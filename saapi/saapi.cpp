#include "saapi.h"

#include <string>
#define WIN32_LEAN_AND_MEAN
#include <windows.h>

HANDLE hPipe;

BOOL APIENTRY DllMain(HMODULE hModule, DWORD ul_reason_for_call, LPVOID lpReserved)
{
  switch (ul_reason_for_call)
  {
  case DLL_PROCESS_ATTACH:
    InitPipe();
    SA_SayW(L"CONNECTED");
    break;

  case DLL_PROCESS_DETACH:
    SA_SayW(L"DISCONNECTED");
    break;

  case DLL_THREAD_ATTACH:
  case DLL_THREAD_DETACH:
    break;
  }
  return TRUE;
}

void InitPipe()
{
  hPipe = CreateFileW(TEXT("\\\\.\\pipe\\d4tts"), GENERIC_WRITE, FILE_SHARE_WRITE, NULL, OPEN_EXISTING, 0, NULL);
}

std::string wcharToString(const wchar_t* wstr) 
{
  std::size_t converted = 0;
  std::size_t len = 0;
  wcstombs_s(&converted, nullptr, 0, wstr, 0);
  len = converted;

  char* buffer = new char[len + 1];
  wcstombs_s(&converted, buffer, len + 1, wstr, len);
  std::string result(buffer);
  delete[] buffer;
  return result;
}

extern "C" bool SA_SayW(const wchar_t* str)
{
  if (!str) return false;

  // Send data to D4TTS app
  std::string narrowStr = wcharToString(str) + "\r\n";
  BOOL result = WriteFile(hPipe, narrowStr.c_str(), strlen(narrowStr.c_str()), nullptr, NULL);
  if (!result) InitPipe();
  return true;
}

extern "C" bool SA_BrlShowTextW(const wchar_t *str) { return true; }

extern "C" bool SA_IsRunning() { return true; }

extern "C" bool SA_StopAudio() { return true; }
