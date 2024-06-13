from http.server import BaseHTTPRequestHandler, HTTPServer
import logging
import json
from urllib.parse import urlparse

import requests
from mesa import Agent, Model
from mesa.space import MultiGrid
from mesa.time import RandomActivation
from mesa.datacollection import DataCollector
import numpy as np
import random
import time

robotsData = []

class TrashAgent(Agent):
    def __init__(self, unique_id, model):
        super().__init__(unique_id, model)
        self.trash_amount = 0

class RobotAgent(Agent):
    def __init__(self, unique_id, model, trash_capacity=5):
        super().__init__(unique_id, model)
        self.trash_capacity = trash_capacity
        self.current_trash = 0
        self.trash_bin_location = None
        self.movements = 0  # Inicializar el atributo movements

    def step(self):
        self.collect_trash()  # Recolectar basura en cada paso
        if self.current_trash >= self.trash_capacity:
            self.move_to_trash_bin()

    def collect_trash(self):
        cell_contents = self.model.grid.get_cell_list_contents([self.pos])
        for agent in cell_contents:
            if isinstance(agent, TrashAgent) and agent.trash_amount > 0:
                while self.current_trash < self.trash_capacity and agent.trash_amount > 0:
                    self.current_trash += 1
                    agent.trash_amount -= 1
                if agent.trash_amount == 0:
                    self.model.grid.remove_agent(agent)
                if self.current_trash == self.trash_capacity:
                    break  # Salir del bucle una vez que la capacidad está llena

    def move_to_trash_bin(self):
        self.move_towards(self.trash_bin_location)
        if self.pos == self.trash_bin_location:
            self.current_trash = 0

    def move(self):
        # Obtener vecinos válidos
        possible_steps = self.model.grid.get_neighborhood(self.pos, moore=True, include_center=False)
        possible_steps = [step for step in possible_steps if not any(isinstance(obj, str) and obj == 'X' for obj in self.model.grid.get_cell_list_contents([step]))]

        # Si hay vecinos disponibles
        if possible_steps:
            # Ordenar los vecinos según la cantidad de basura y la distancia al contenedor de basura
            possible_steps.sort(key=lambda step: (sum(isinstance(obj, TrashAgent) for obj in self.model.grid.get_cell_list_contents([step])), self.model.grid.get_distance(step, self.trash_bin_location)))
            
            # Elegir el vecino con más basura y más cercano al contenedor de basura
            new_position = possible_steps[-1]
            
            # Mover el agente a la nueva posición
            self.model.grid.move_agent(self, new_position)
            self.movements += 1


    def move_towards(self, destination):
        if destination is None:
            return
        if self.pos == destination:
            return
        possible_steps = self.model.grid.get_neighborhood(self.pos, moore=True, include_center=False)
        possible_steps = [step for step in possible_steps if not any(isinstance(obj, str) and obj == 'X' for obj in self.model.grid.get_cell_list_contents([step]))]
        min_distance = float('inf')
        best_step = None
        for step in possible_steps:
            distance = self.model.grid.get_distance(step, destination)
            if distance < min_distance:
                min_distance = distance
                best_step = step
        if best_step:
            self.model.grid.move_agent(self, best_step)
            self.movements += 1

class OficinaModel(Model):
    def __init__(self, workspace_file):
        super().__init__()
        self.num_robots = 5
        self.total_steps = 0  # Inicializa el contador de pasos

        # Leer configuración del espacio de trabajo desde el archivo
        self.height, self.width, self.workspace = read_workspace(workspace_file)

        # Crear el grid y otros componentes
        self.grid = MultiGrid(self.width, self.height, True)
        self.schedule = RandomActivation(self)
        self.datacollector = DataCollector(
            {"Total Movements": lambda m: self.total_movements()}
        )
        self.grid_history = []
        self.trash_bin_location = None
        self.trash_map = {}

        agent_id = 0
        robot_count = 0
        robot_positions = []

        for y in range(self.height):
            for x in range(self.width):
                if (x, y) in self.workspace:
                    cell = self.workspace[(x, y)]
                    if isinstance(cell, str) and cell.isdigit():
                        self.trash_map[(x, y)] = int(cell)
                        trash_agent = TrashAgent(agent_id, self)
                        self.schedule.add(trash_agent)
                        self.grid.place_agent(trash_agent, (x, y))
                        agent_id += 1
                    elif cell == 'P':
                        self.trash_bin_location = (x, y)
                    elif cell == 'S':
                        if robot_count < 5:
                            robot = RobotAgent(agent_id, self)
                            self.schedule.add(robot)
                            self.grid.place_agent(robot, (x, y))
                            agent_id += 1
                            robot_count += 1
                            robot_positions.append((x, y))
                        else:
                            raise ValueError("More than 5 starting positions ('S') for robots.")

        if len(robot_positions) != len(set(robot_positions)):
            raise ValueError("Robots are overlapping in initial positions.")

        for _ in range(self.num_robots):
            robot = RobotAgent(agent_id, self)
            self.schedule.add(robot)
            empty_pos = self.find_empty()
            self.grid.place_agent(robot, empty_pos)
            agent_id += 1

    def step(self):
        print("Ejecutando paso del modelo...")
        self.schedule.step()
        self.record_grid()
        self.datacollector.collect(self)
        self.total_steps += 1  # Incrementa el contador de pasos

        # Enviar datos de robots al servidor
        robot_data = self.collect_robot_data()
        data = {
            'time_step': self.schedule.time,
            'robots': robot_data
        }
        print(data)
        robotsData.append(data['robots'])
        print(f"Número total de pasos de simulación realizados: {self.total_steps}")

    def collect_robot_data(self):
        robot_data = []
        for agent in self.schedule.agents:
            if isinstance(agent, RobotAgent):
                robot_data.append({
                    'id': agent.unique_id,
                    'x': agent.pos[0],
                    'y': agent.pos[1],
                    'movements': agent.movements
                })
        return robot_data

    def find_empty(self):
        empty_cells = list(self.grid.empties)
        if not empty_cells:
            raise ValueError("No hay posiciones vacías disponibles en la malla.")
        return random.choice(empty_cells)
    
    def check_clean(self):
        for cell in self.grid.coord_iter():
            cell_content, coord = cell
            if any(isinstance(obj, TrashAgent) for obj in cell_content):
                return False
        return True

    def record_grid(self):
        grid_data = []
        for y in range(self.height):
            row = []
            for x in range(self.width):
                cell_content = self.grid[x][y]
                if any(isinstance(agent, RobotAgent) for agent in cell_content):
                    row.append([5, 0, 0])  # Robot presente
                elif any(isinstance(agent, TrashAgent) for agent in cell_content):
                    row.append([1, 0, 0])  # Basura presente
                elif (x, y) == self.trash_bin_location:
                    row.append([-1, 0, 0])  # Papelera de reciclaje
                else:
                    row.append([0, 0, 1])  # Celda vacía
            grid_data.append(row)
        self.grid_history.append(grid_data)

    def total_movements(self):
        total = 0
        for agent in self.schedule.agents:
            if isinstance(agent, RobotAgent):
                total += agent.movements
        return total

def read_workspace(file_path):
    with open(file_path, 'r') as file:
        lines = file.readlines()
        n, m = map(int, lines[0].strip().split())
        trash_map = {}
        for i in range(1, n + 1):
            row = lines[i].strip().split()
            for j in range(len(row)):
                trash_map[(j, i - 1)] = row[j] 
        return n, m, trash_map

def execAgent():
    global robotsData
    # Leer espacio de configuración
    file_path = '../Robots/ActividadIntegradora/input1.txt'
    width, height, workspace = read_workspace(file_path)

    # Inicializar y correr el modelo
    model = OficinaModel(file_path)

    # Ejecutar el modelo hasta que esté limpio
    while model.total_steps == 0 or not model.check_clean():
        print("Ejecutando paso del modelo...")
        model.step()

    print("La simulación ha terminado.")
    print(f"Número total de pasos de simulación realizados: {model.total_steps}")
    
    # Retornar el modelo y los datos
    return model.total_steps, robotsData

class Server(BaseHTTPRequestHandler):
    total_steps = 0
    robots_data = []

    def _set_response(self, status_code=200):
        self.send_response(status_code)
        self.send_header('Content-type', 'application/json')
        self.end_headers()

    def do_GET(self):
        parsed_path = urlparse(self.path)
        path = parsed_path.path

        if path == '/steps':
            self.handle_steps()
        elif path.startswith('/default/'):
            self.handle_default(path)
        else:
            self.handle_not_found()

    def handle_steps(self):
        steps = Server.total_steps
        stepCount = {
            "data": steps
        }
        self._set_response()
        self.wfile.write(json.dumps(stepCount).encode('utf-8'))

    def handle_default(self, path):
        parts = path.split('/')
        if len(parts) < 3 or not parts[2].isdigit():
            self._set_response(400)
            response = {
                "error": "Missing 'id' parameter in URL"
            }
            self.wfile.write(json.dumps(response).encode('utf-8'))
        else:
            id = int(parts[2])
            if id >= len(Server.robots_data):
                self._set_response(404)
                response = {
                    "error": "Step not found"
                }
                self.wfile.write(json.dumps(response).encode('utf-8'))
                return

            datosSteps = Server.robots_data[id]
            # Formatear los datos en una lista de listas de coordenadas [x, y, z]
            position_data = [[robot['x'], robot['y'], 0] for robot in datosSteps]

            response = {
                "data": position_data
            }
            self._set_response()
            self.wfile.write(json.dumps(response).encode('utf-8'))

    def handle_not_found(self):
        self._set_response(404)
        response = {
            "error": "Not found"
        }
        self.wfile.write(json.dumps(response).encode('utf-8'))

def run(server_class=HTTPServer, handler_class=Server, port=8585):
    logging.basicConfig(level=logging.INFO)
    server_address = ('', port)
    httpd = server_class(server_address, handler_class)
    logging.info("Starting httpd...\n")
    
    # Ejecutar el agente y almacenar los resultados en las variables de clase del manejador
    Server.total_steps, Server.robots_data = execAgent()

    try:
        httpd.serve_forever()
    except KeyboardInterrupt:
        pass
    httpd.server_close()
    logging.info("Stopping httpd...\n")

if __name__ == '__main__':
    from sys import argv

    if len(argv) == 2:
        run(port=int(argv[1]))
    else:
        run()