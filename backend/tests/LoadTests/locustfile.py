from locust import HttpUser, task
import os
import requests

apiHost = os.environ['load_test_api_host']
email = os.environ['load_test_username']
password = os.environ['load_test_password']

loginResponse = requests.post(f'{apiHost}/api/user/login', json={"email": email, "password":password}, verify=False)
token = loginResponse.json()['jwtToken']

class ScrumPokerApiLoadTest(HttpUser):
    host = apiHost

    @task
    def refresh_token(self):
        self.client.get("/api/user/refreshtoken")
    
    @task
    def authenticate(self):
        self.client.post("/api/user/authenticate")
    
    @task
    def get_room_stories(self):
        self.client.get("/api/room/1/stories")
    
    @task
    def get_story_by_id(self):
        self.client.get("/api/story/1")

    def on_start(self):
        self.client.verify = False
        self.client.headers = { 'Authorization': 'Bearer ' + token }
