from mesa import Agent, Model 
from mesa.space import MultiGrid
from mesa.time import RandomActivation
from mesa.datacollection import DataCollector
import numpy as np
import random
import time

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
            cell_content, coord = cell
            if 'P' in cell_content:
                self.trash_bin_location = coord

    def step(self):
        if self.current_trash >= self.trash_capacity:
            self.move_to_trash_bin()
        else:
            self.clean_or_move()

    def move_to_trash_bin(self):
        self.move_towards(self.trash_bin_location)
        if self.pos == self.trash_bin_location:
            self.current_trash = 0

    def clean_or_move(self):
        cell_contents = self.model.grid.get_cell_list_contents([self.pos])
        trash_agents = [obj for obj in cell_contents if isinstance(obj, TrashAgent)]
        if trash_agents:
            self.clean_trash(trash_agents[0])
        else:
            self.move()

    def clean_trash(self, trash_agent):
        if trash_agent.trash_amount > 0:
            cleaned_amount = min(trash_agent.trash_amount, self.trash_capacity - self.current_trash)
            self.current_trash += cleaned_amount
            trash_agent.trash_amount -= cleaned_amount
            if trash_agent.trash_amount <= 0:
                self.model.grid.remove_agent(trash_agent)

    def move(self):
        possible_steps = self.model.grid.get_neighborhood(self.pos, moore=True, include_center=False)
        possible_steps = [step for step in possible_steps if 'X' not in self.model.grid.get_cell_list_contents([step])]
        if possible_steps:
            new_position = random.choice(possible_steps)
            self.model.grid.move_agent(self, new_position)
            self.movements += 1

    def move_towards(self, destination):
        if self.pos == destination:
            return
        possible_steps = self.model.grid.get_neighborhood(self.pos, moore=True, include_center=False)
        possible_steps = [step for step in possible_steps if 'X' not in self.model.grid.get_cell_list_contents([step])]
        # Find the step closest to the destination
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

class AlmacenModel(Model):
    def __init__(self, width, height, num_robots, workspace):
        super().__init__()
        self.num_robots = num_robots
        self.grid = MultiGrid(width, height, True)
        self.schedule = RandomActivation(self)
        self.datacollector = DataCollector(
            {"Total Movements": lambda m: self.total_movements()}
        )
        self.grid_history = []

        # Create agents based on workspace configuration
        agent_id = 0
        for y in range(height):
            for x in range(width):
                if y < len(workspace) and x < len(workspace[y]):
                    cell = workspace[y, x]
                    if cell == 'R':
                        r = RobotAgent(agent_id, self)
                        self.schedule.add(r)
                        self.grid.place_agent(r, (x, y))
                        agent_id += 1
                    elif cell.isdigit():
                        trash_amount = int(cell)
                        trash_agent = TrashAgent(agent_id, self, trash_amount)
                        self.schedule.add(trash_agent)
                        self.grid.place_agent(trash_agent, (x, y))
                        agent_id += 1

    def step(self):
        self.schedule.step()
        self.record_grid()
        self.datacollector.collect(self)

    def check_clean(self):
        for cell in self.grid.coord_iter():
            cell_content, coord = cell
            if any(isinstance(obj, TrashAgent) for obj in cell_content):
                return False
        return True

    def record_grid(self):
        grid_data = np.zeros((self.grid.width, self.grid.height))
        for cell in self.grid.coord_iter():
            cell_content, coord = cell
            x, y = coord
            for agent in cell_content:
                if isinstance(agent, RobotAgent):
                    grid_data[y][x] = 8  # Represent robots with a higher value
                elif isinstance(agent, TrashAgent):
                    grid_data[y][x] = agent.trash_amount
        self.grid_history.append(grid_data)

    def total_movements(self):
        return sum([agent.movements for agent in self.schedule.agents if isinstance(agent, RobotAgent)])

# Ruta al archivo con espacio de configuracion
file_path = '../Robots/ActividadIntegradora/input1.txt'  # Cambia esto por la ruta real de tu archivo

# Leer espacio de configuracion
width, height, workspace = read_workspace(file_path)

# Inicializar y correr el modelo
model = AlmacenModel(width, height, 5, workspace)
start_time = time.time()
for i in range(1000):
    model.step()
    if model.check_clean():
        break
end_time = time.time()

# Gather results
time_needed = end_time - start_time
total_movements = model.total_movements()

print(f"Time needed: {time_needed} seconds")
print(f"Total movements: {total_movements}")

# Potential Strategy Analysis:
# One potential strategy to improve the efficiency of the robots could be to implement a more intelligent movement pattern.
# For example, robots could prioritize moving to cells with the most trash or implement pathfinding algorithms to navigate efficiently.
# Additionally, implementing a communication system between robots to avoid redundant work and collisions could also reduce the overall time and movements required.
