#include "MyOGL.h"
#include "Render.h"



LRESULT CALLBACK WndProc(HWND hWnd, UINT message, WPARAM wParam, LPARAM lParam);



extern "C" __declspec(dllexport) void* CreateWnd(void *_hInstanse, HWND parent)
{

	TCHAR szWindowClass[100] = L"wpf_ogl";

	HINSTANCE hInstance = (HINSTANCE)_hInstanse;

	WNDCLASSEX wcex;

	wcex.cbSize = sizeof(WNDCLASSEX);

	wcex.style = CS_HREDRAW | CS_VREDRAW | CS_OWNDC;
	wcex.lpfnWndProc = WndProc;
	wcex.cbClsExtra = 0;
	wcex.cbWndExtra = 0;
	wcex.hInstance = hInstance;
	//wcex.hIcon = LoadIcon(hInstance, MAKEINTRESOURCE(IDI_OPENGLREMEMBERPROJ));
	wcex.hCursor = LoadCursor(NULL, IDC_ARROW);
	wcex.hbrBackground = (HBRUSH)(COLOR_WINDOW + 1);
	wcex.lpszMenuName = 0;
	wcex.lpszClassName = szWindowClass;
	//wcex.hIconSm = LoadIcon(wcex.hInstance, MAKEINTRESOURCE(IDI_SMALL));
	RegisterClassEx(&wcex);

	HWND hWnd;
	hWnd = CreateWindow(szWindowClass, L"", WS_CHILD | WS_VISIBLE,
		0, 0, 100, 100, parent, NULL, hInstance, NULL);

	if (!hWnd)
	{
		MessageBox(0, L"qwe", L"asd", 0);
		return 0;
	}

	ShowWindow(hWnd, SW_SHOW);
	UpdateWindow(hWnd);

	return (void *)hWnd;
}

OpenGL gl;
LRESULT CALLBACK WndProc(HWND hWnd, UINT message, WPARAM wParam, LPARAM lParam)
{
	int wmId, wmEvent;
	PAINTSTRUCT ps;
	HDC hdc;

	switch (message)
	{
	case WM_CREATE:
	{
		//
		gl.setHWND(hWnd);

		//привязываем собития к классу OpenGL

		gl.initFunc.push_back(initRender);
		gl.renderFunc.push_back(Render);


		gl.mouseFunc.push_back(mouseEvent);
		gl.wheelFunc.push_back(mouseWheelEvent);
		gl.keyDownFunc.push_back(keyDownEvent);
		gl.keyUpFunc.push_back(keyUpEvent);
		gl.resizeFunc = resizeEvent;

		gl.init();


		DWORD r = SetTimer(hWnd, 1213, 16, (TIMERPROC)NULL);
		//Beep(3000, 300);
		break;
	}


	case WM_KEYDOWN:
	{
		gl.keyDownEvent(wParam);
		break;
	}

	case WM_KEYUP:
	{
		gl.keyUpEvent(wParam);
		break;
	}

	case WM_LBUTTONDOWN:
	{
		//gl.keyDownEvent(VK_LBUTTON);
		SetFocus(hWnd);
		break;
	}

	case WM_RBUTTONDOWN:
	{
		//gl.keyDownEvent(VK_RBUTTON);
		SetFocus(hWnd);
		break;
	}


	case WM_TIMER:


		switch (wParam)
		{
		case 1213:
			gl.render();
			break;
		default:
			break;
		}

		break;

	case WM_MOUSEMOVE:
		SetFocus(hWnd);
		gl.mouseMovie(LOWORD(lParam), HIWORD(lParam));
		break;

	case WM_MOUSEWHEEL:

		gl.wheelEvent(GET_WHEEL_DELTA_WPARAM(wParam));
		break;
	/*case WM_PAINT:
		hdc = BeginPaint(hWnd, &ps);
		EndPaint(hWnd, &ps);

		break;*/
	case WM_SIZE:
		gl.resize(LOWORD(lParam), HIWORD(lParam));
		//Beep(5000, 1000);
		break;
	case WM_DESTROY:
		PostQuitMessage(0);
		break;
	default:
		return DefWindowProc(hWnd, message, wParam, lParam);
	}
	return 0;
}