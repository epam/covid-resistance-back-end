import argparse
import getpass
import requests


def main():
    auth_token = get_token()
    security_ids = generate_security_ids(auth_token)
    for security_id in security_ids:
        print(security_id)


def get_token():
    payload = {"username": username, "password": password}
    response = requests.post(url=f"{service_url}/admin/login", json=payload)
    return response.json()["token"]["accessToken"]


def generate_security_ids(auth_token):
    payload = {"numberOfCodes": number_of_codes, "codeLength": code_length, "comment": comment}
    headers = {"Authorization": f"Bearer {auth_token}"}
    response = requests.post(url=f"{service_url}/admin/generatehealthsecurityids", json=payload, headers=headers)
    return response.json()["healthSecurityIds"]


def get_argument(name, is_password=False):
    value = args[name]
    if not value:
        if is_password:
            value = getpass.getpass()
        else:
            value = input(f"Enter {name}: ")
    return value


if __name__ == "__main__":
    parser = argparse.ArgumentParser()
    parser.add_argument("-s", "--service-url", required=True)
    parser.add_argument("-u", "--username")
    parser.add_argument("-p", "--password")
    parser.add_argument("-n", "--number-of-codes", required=True, type=int)
    parser.add_argument("-l", "--code-length", type=int, default=8)
    parser.add_argument("-c", "--comment", required=True)
    args = vars(parser.parse_args())

    service_url = args['service_url']
    username = get_argument("username")
    password = get_argument('password', True)
    number_of_codes = get_argument("number_of_codes")
    code_length = get_argument("code_length")
    comment = get_argument("comment")
    main()
