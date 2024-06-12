import requests
from mesa import Agent, Model
from mesa.space import MultiGrid
from mesa.time import RandomActivation
from mesa.datacollection import DataCollector
import numpy as np
import random
import time
import json


# Función para leer el archivo de configuración del espacio de trabajo
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
        self.movements = 0  # Agregar esta línea para inicializar el atributo movements

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
        possible_steps = self.model.grid.get_neighborhood(self.pos, moore=True, include_center=False)
        possible_steps = [step for step in possible_steps if not any(isinstance(obj, str) and obj == 'X' for obj in self.model.grid.get_cell_list_contents([step]))]
        if possible_steps:
            new_position = random.choice(possible_steps)
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
        self.num_robots = 5  # Siempre habrá 5 robots
        self.grid = None
        self.schedule = RandomActivation(self)
        self.datacollector = DataCollector(
            {"Total Movements": lambda m: self.total_movements()}
        )
        self.grid_history = []
        self.trash_bin_location = None
        self.trash_map = {}  # Diccionario para almacenar la cantidad de basura en cada celda

        # Leer configuración del espacio de trabajo desde el archivo
        self.height, self.width, self.workspace = read_workspace(workspace_file)

        agent_id = 0
        robot_count = 0  # Contador de robots creados
        robot_positions = []  # Lista para rastrear posiciones de robots

        for y in range(self.height):
            for x in range(self.width):
                if (x, y) in self.workspace:
                    cell = self.workspace[(x, y)]
                    if isinstance(cell, str) and cell.isdigit():
                        self.trash_map[(x, y)] = int(cell)
                        trash_agent = TrashAgent(agent_id, self)
                        self.schedule.add(trash_agent)
                        if self.grid is None:
                            self.grid = MultiGrid(self.width, self.height, True)
                        self.grid.place_agent(trash_agent, (x, y))
                        agent_id += 1
                    elif cell == 'P':
                        self.trash_bin_location = (x, y)
                    elif cell == 'S':
                        if robot_count < 5:
                            robot = RobotAgent(agent_id, self)
                            self.schedule.add(robot)
                            if self.grid is None:
                                self.grid = MultiGrid(self.width, self.height, True)
                            self.grid.place_agent(robot, (x, y))
                            agent_id += 1
                            robot_count += 1
                        else:
                            raise ValueError("More than 5 starting positions ('S') for robots.")

        # Comprobar si los robots están encimados en la posición inicial
        if len(robot_positions) != len(set(robot_positions)):
            raise ValueError("Robots are overlapping in initial positions.")

        for _ in range(self.num_robots):  
            robot = RobotAgent(agent_id, self)
            self.schedule.add(robot)
            empty_pos = self.find_empty()
            if self.grid is None:
                self.grid = MultiGrid(self.width, self.height, True)
            self.grid.place_agent(robot, empty_pos)
            agent_id += 1

    def find_empty(self):
        while True:
            pos = (random.randrange(self.width), random.randrange(self.height))
            if self.grid is not None and self.grid.is_cell_empty(pos):
                return pos

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

    def step(self):
        self.schedule.step()
        self.record_grid()
        self.datacollector.collect(self)
        print("Se ha ejecutado un paso del modelo.")

            # Send robot data to the server
        robot_data = self.collect_robot_data()
        data = {
            'time_step': self.schedule.time,
            'robots': robot_data
        }
        try:
            requests.post("http://localhost:8585/update", json=data)
        except requests.exceptions.RequestException as e:
            print(f"Error sending data: {e}")

    def check_clean(self):
        for cell in self.grid.coord_iter():
            cell_content, coord = cell
            if any(isinstance(obj, TrashAgent) for obj in cell_content):
                return False
        return True

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
                trash_map[row[j]] = (j, i - 1)
        return n, m, trash_map


# Ruta al archivo con espacio de configuración
file_path = '../Robots/ActividadIntegradora/input1.txt'  # Cambia esto por la ruta real de tu archivo

# Leer espacio de configuración
width, height, workspace = read_workspace(file_path)

# Inicializar y correr el modelo
model = OficinaModel(file_path)

# Ejecutar el modelo hasta que esté limpio
while not model.check_clean():
    model.step()

# Obtener el número total de pasos de simulación realizados
total_steps = model.schedule.steps

print("La simulación ha terminado.")
print(f"Número total de pasos de simulación realizados: {total_steps}")