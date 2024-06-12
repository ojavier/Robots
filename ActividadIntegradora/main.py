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
def read_workspace(file_path):
    try:
        with open(file_path, 'r') as file:
            lines = file.readlines()

        # Leer dimensiones
        n, m = map(int, lines[0].strip().split())

        # Crear la matriz de la oficina
        workspace = []
        for line in lines[1:]:
            workspace.append(line.strip().split())

        return n, m, np.array(workspace)
    except FileNotFoundError:
        print(f"Error: El archivo '{file_path}' no se encontró. Verifica la ruta del archivo.")
        raise


class TrashAgent(Agent):
    def __init__(self, unique_id, model, trash_amount):
        super().__init__(unique_id, model)
        self.trash_amount = trash_amount


class RobotAgent(Agent):
    def __init__(self, unique_id, model, trash_capacity=5):
        super().__init__(unique_id, model)
        self.trash_capacity = trash_capacity
        self.current_trash = 0
        self.movements = 0
        self.trash_bin_location = None
        for cell in self.model.grid.coord_iter():
            cell_content, coord = cell[0], cell[1]
            if 'P' in cell_content:
                self.trash_bin_location = coord

    def step(self):
        if self.current_trash >= self.trash_capacity:
            self.move_to_trash_bin()
        else:
            self.collect_trash()

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
                    self.move_to_trash_bin()
                    break
        else:
            self.move()

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
        if self.pos == destination:
            return
        possible_steps = self.model.grid.get_neighborhood(self.pos, moore=True, include_center=False)
        possible_steps = [step for step in possible_steps if not any(isinstance(obj, str) and obj == 'X' for obj in self.model.grid.get_cell_list_contents([step]))]
        min_distance = float('inf')
        best_step = None
        for step in possible_steps:
            distance = np.linalg.norm(np.array(step) - np.array(destination))
            if distance < min_distance:
                min_distance = distance
                best_step = step
        if best_step:
            self.model.grid.move_agent(self, best_step)
            self.movements += 1


class OficinaModel(Model):
    def __init__(self, width, height, num_robots, workspace):
        super().__init__()
        self.num_robots = num_robots
        self.grid = MultiGrid(width, height, True)
        self.schedule = RandomActivation(self)
        self.datacollector = DataCollector(
            {"Total Movements": lambda m: self.total_movements()}
        )

        # Create agents based on workspace configuration
        agent_id = 0
        robot_start_positions = []
        for y in range(height):
            for x in range(width):
                if y < len(workspace) and x < len(workspace[y]):
                    cell = workspace[y, x]
                    if cell.isdigit():
                        trash_amount = int(cell)
                        trash_agent = TrashAgent(agent_id, self, trash_amount)
                        self.schedule.add(trash_agent)
                        self.grid.place_agent(trash_agent, (x, y))
                        agent_id += 1
                    elif cell == 'P':
                        self.trash_bin_location = (x, y)
                    elif cell == 'S':
                        robot_start_positions.append((x, y))

        # Create robots at 'S' positions
        for i in range(num_robots):
            if i < len(robot_start_positions):
                start_pos = robot_start_positions[i]
                r = RobotAgent(agent_id, self)
                self.schedule.add(r)
                self.grid.place_agent(r, start_pos)
                agent_id += 1
            else:
                raise ValueError("Not enough starting positions ('S') for all robots.")

    def step(self):
        self.schedule.step()
        self.datacollector.collect(self)
        if self.check_clean():
            self.running = False

    def check_clean(self):
        for cell in self.grid.coord_iter():
            cell_content, coord = cell
            if any(isinstance(obj, TrashAgent) for obj in cell_content):
                return False
        return True

    def total_movements(self):
        return sum([agent.movements for agent in self.schedule.agents if isinstance(agent, RobotAgent)])


# Ruta al archivo con espacio de configuración
file_path = '../Robots/ActividadIntegradora/input1.txt'  # Cambia esto por la ruta real de tu archivo

# Leer espacio de configuración
width, height, workspace = read_workspace(file_path)

# Inicializar y correr el modelo
model = OficinaModel(width, height, 5, workspace)
start_time = time.time()
for i in range(1000):  # Limitar a 1000 pasos o hasta que se limpie toda la basura
    model.step()
    if model.check_clean():
        break
end_time = time.time()

# Gather results
time_needed = end_time - start_time
total_movements = model.total_movements()

print(f"Time needed: {time_needed} seconds")
print(f"Total movements: {total_movements}")