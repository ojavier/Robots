from http.server import BaseHTTPRequestHandler, HTTPServer
import logging
import json

class Server(BaseHTTPRequestHandler):
    
    def _set_response(self):
        self.send_response(200)
        self.send_header('Content-type', 'application/json')
        self.end_headers()
        
    def do_GET(self):
        # Construye el objeto de posición
        position = {
            "x": 1,
            "y": 2,
            "z": 3
        }

        # Envía la respuesta como JSON
        self._set_response()
        self.wfile.write(json.dumps(position).encode('utf-8'))

    def do_POST(self):
        # Construye el objeto de posición
        position = {
            "x" : 1,
            "y" : 2,
            "z" : 3
        }

        # Envía la respuesta como JSON
        self._set_response()
        self.wfile.write(json.dumps(position).encode('utf-8'))

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
