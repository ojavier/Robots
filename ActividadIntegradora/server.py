from http.server import BaseHTTPRequestHandler, HTTPServer
import logging
import json
from urllib.parse import urlparse

class Server(BaseHTTPRequestHandler):
    robot_data = []

    def _set_response(self):
        self.send_response(200)
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
        #steps es un entero
        steps = 5
        stepCount = {
            "data": steps
        }
        self._set_response()
        self.wfile.write(json.dumps(stepCount).encode('utf-8'))
        
    def handle_default(self, path):
        parts = path.split('/')
        if len(parts) < 3 or not parts[2]:
            self._set_response(400)
            response = {
                "error": "Missing 'id' parameter in URL"
            }
        else:
            id = parts[2]
            datosSteps = []
            #[[5, 0, 0], [1, 0, 0], [0, 0, 1], [1, 0, 1], [-1, 0, 0]]
            
            position = {
                "data": datosSteps[id]
            }
            self._set_response()
            response = position
        
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
