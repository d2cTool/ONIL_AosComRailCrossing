// UnmCpp.cpp: определяет точку входа для консольного приложения.
//

#include <cstdlib>
#include <iostream>
#include <fstream>
#include <ctime>
#include <Windows.h>
#include <chrono>
#include <thread>
#include <sstream>
#include <string.h>
#include <limits>

//using namespace std::chrono_literals;

typedef bool(*functionPointer1)(const char* serial1);
typedef bool(*functionPointer2)();
typedef bool(*functionPointer3)(int btn, byte state);
typedef bool(*functionPointer4)(int firmware);
typedef bool(*functionPointer5)(const char* msg);

extern "C" {
	void InitDevice();
	void Init();
	void Stop();
	byte SetLamp(int, byte);
	int GetFirmware();
	int UpdateFirmware(const char*);
	void Send(const char*);

	bool OnConnectCallbackFunction(functionPointer1);
	bool OnDisconnectCallbackFunction(functionPointer2);
	bool OnStateChangedCallbackFunction(functionPointer3);
	bool OnChangedFirmwareCallbackFunction(functionPointer4);
}

bool OnConnectCallback(const char* str1) {
	std::cout << "Connect: device serial: " << str1 << std::endl;
	InitDevice();
	return true;
}

bool OnDisconnectCallback() {
	std::cout << "Disconnect" << std::endl;
	return true;
}

bool OnStateChangedCallback(int btn, byte state) {
	std::cout << "New state: btn: " << btn << ": " << (int)state << std::endl;
	return true;
}

bool UpdateFirmwareCallback(int version) {
	std::cout << "Firmware version: " << version << std::endl;
	return true;
}

void InitCallback(){
	OnConnectCallbackFunction(&OnConnectCallback);
	OnDisconnectCallbackFunction(&OnDisconnectCallback);
	OnStateChangedCallbackFunction(&OnStateChangedCallback);
	OnChangedFirmwareCallbackFunction(&UpdateFirmwareCallback);
}

int main()
{
	InitCallback();
	std::cout << "InitCallback" << std::endl;

	Init();
	std::cout << "Init" << std::endl;

	std::string input;
	std::string l;
	int lla;
	int x;
	byte bb;

	while (true)
	{
		getline(std::cin, input);
		x = std::stoi(input);

		switch (x)
		{
		case 1:
			std::cout << "Select lamp: " << std::endl;
			getline(std::cin, l);
			lla = std::stoi(l);
			SetLamp(lla, 1);
			break;
		case 2:
			std::cout << "Firmware: " << GetFirmware() << std::endl;
			break;
		case 3:
			//UpdateFirmware(hexFile3.c_str());
			break;
		case 4:
			std::cout << "Enter command: ";
			getline(std::cin, l);
			Send(l.c_str());
			break;
		default:
			break;
		}
	}

	return EXIT_SUCCESS;
}
