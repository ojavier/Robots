from http.server import BaseHTTPRequestHandler, HTTPServer
import logging
import json

class Server(BaseHTTPRequestHandler):
    robot_data = []

    def _set_response(self):
        self.send_response(200)
        self.send_header('Content-type', 'application/json')
        self.end_headers()

    def do_GET(self):
        # Return the latest robot data as JSON
        self._set_response()
        response = {
            'time_step': 1,  # Example static time_step, update as necessary
            'robots': Server.robot_data
        }
        response_json = json.dumps(response)
        logging.info("Sending JSON response: %s", response_json)
        self.wfile.write(response_json.encode('utf-8'))

    def do_POST(self):
        content_length = int(self.headers['Content-Length'])
        post_data = self.rfile.read(content_length)
        logging.info("Received POST data: %s", post_data.decode('utf-8'))

        try:
            Server.robot_data = json.loads(post_data)
            self._set_response()
            self.wfile.write(json.dumps({'status': 'success'}).encode('utf-8'))
        except json.JSONDecodeError as e:
            logging.error("JSONDecodeError: %s", e)
            self.send_response(400)
            self.end_headers()
            self.wfile.write(json.dumps({'status': 'error', 'message': str(e)}).encode('utf-8'))

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
