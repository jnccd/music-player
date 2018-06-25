#include <stdio.h>

extern "C"
{
	__declspec(dllexport) void DisplayHelloFromDLL()
	{
		printf("Hello\n");
	}

	__declspec(dllexport) void DisplayMessageFromDLL(char message[])
	{
		printf(message);
	}
}