#include "MyShaders.h"


PFNGLDELETEOBJECTARBPROC glDeleteObjectARB;
PFNGLGETHANDLEARBPROC glGetHandleARB;
PFNGLDETACHOBJECTARBPROC glDetachObjectARB;
PFNGLCREATESHADEROBJECTARBPROC glCreateShaderObjectARB;
PFNGLSHADERSOURCEARBPROC glShaderSourceARB;
PFNGLCOMPILESHADERARBPROC glCompileShaderARB;
PFNGLCREATEPROGRAMOBJECTARBPROC glCreateProgramObjectARB;
PFNGLATTACHOBJECTARBPROC glAttachObjectARB;
PFNGLLINKPROGRAMARBPROC glLinkProgramARB;
PFNGLVALIDATEPROGRAMARBPROC glValidateProgramARB;
PFNGLUSEPROGRAMOBJECTARBPROC glUseProgramObjectARB;
PFNGLGETOBJECTPARAMETERIVARBPROC glGetObjectParameterivARB;
PFNGLGETINFOLOGARBPROC glGetInfoLogARB;
PFNGLUNIFORM1FARBPROC glUniform1fARB;
PFNGLUNIFORM1IARBPROC glUniform1iARB;
PFNGLUNIFORM2IARBPROC glUniform2iARB;
PFNGLUNIFORM3FARBPROC glUniform3fARB;
PFNGLUNIFORM2FARBPROC glUniform2fARB;
PFNGLUNIFORM4FARBPROC glUniform4fARB;
PFNGLUNIFORMMATRIX4FVPROC glUniformMatrix4fv;
PFNGLGETUNIFORMLOCATIONARBPROC glGetUniformLocationARB;
PFNGLACTIVETEXTUREPROC glActiveTexture;

std::string errStr;
extern "C" __declspec(dllexport) char * getErrStr()
{
	char *c = (char*)CoTaskMemAlloc((errStr.length())*sizeof(char)+1);
	strcpy(c, errStr.data() );
	return c;
}

extern "C" __declspec(dllexport) int errLength()
{
	return errStr.length();
}
int loadShader(LPWSTR filename, char **shaderSrc, int *programLength)
{
	
		//открываем файл
		HANDLE file = CreateFile(filename, GENERIC_READ, 0, NULL, OPEN_EXISTING, FILE_ATTRIBUTE_NORMAL, NULL);

		LARGE_INTEGER size;

		//узнаем размер файла
		GetFileSizeEx(file, &size);

		*programLength = (int)size.LowPart;

		//выделяем память под буфер для считывания шейдера
		*shaderSrc = (char *)malloc(*programLength*sizeof(char));
		int sss = *programLength*sizeof(char);

		DWORD nBytesRead = 0;
		//считываем шейдер  в буфер
		ReadFile(file, *shaderSrc, *programLength, &nBytesRead, 0);
		CloseHandle(file);
		return 1;
	

}
bool init = false;
void initShadersFunctions()
{
	//наполняем делегаты адресами на функции
	glCreateShaderObjectARB = (PFNGLCREATESHADEROBJECTARBPROC)wglGetProcAddress("glCreateShaderObjectARB");
	glCreateProgramObjectARB = (PFNGLCREATEPROGRAMOBJECTARBPROC)wglGetProcAddress("glCreateProgramObjectARB");
	glAttachObjectARB = (PFNGLATTACHOBJECTARBPROC)wglGetProcAddress("glAttachObjectARB");
	glDetachObjectARB = (PFNGLDETACHOBJECTARBPROC)wglGetProcAddress("glDetachObjectARB");
	glDeleteObjectARB = (PFNGLDELETEOBJECTARBPROC)wglGetProcAddress("glDeleteObjectARB");
	glShaderSourceARB = (PFNGLSHADERSOURCEARBPROC)wglGetProcAddress("glShaderSourceARB");
	glCompileShaderARB = (PFNGLCOMPILESHADERARBPROC)wglGetProcAddress("glCompileShaderARB");
	glLinkProgramARB = (PFNGLLINKPROGRAMARBPROC)wglGetProcAddress("glLinkProgramARB");
	glValidateProgramARB = (PFNGLVALIDATEPROGRAMARBPROC)wglGetProcAddress("glValidateProgramARB");
	glUseProgramObjectARB = (PFNGLUSEPROGRAMOBJECTARBPROC)wglGetProcAddress("glUseProgramObjectARB");
	glGetObjectParameterivARB = (PFNGLGETOBJECTPARAMETERIVARBPROC)wglGetProcAddress("glGetObjectParameterivARB");
	glGetInfoLogARB = (PFNGLGETINFOLOGARBPROC)wglGetProcAddress("glGetInfoLogARB");


	glUniform1fARB = (PFNGLUNIFORM1FARBPROC)wglGetProcAddress("glUniform1fARB");
	glUniform1iARB = (PFNGLUNIFORM1IARBPROC)wglGetProcAddress("glUniform1iARB");
	glUniform2iARB = (PFNGLUNIFORM2IARBPROC)wglGetProcAddress("glUniform2iARB");
	glUniform3fARB = (PFNGLUNIFORM3FARBPROC)wglGetProcAddress("glUniform3fARB");
	glUniform4fARB = (PFNGLUNIFORM4FARBPROC)wglGetProcAddress("glUniform4fARB");
	glUniform2fARB = (PFNGLUNIFORM2FARBPROC)wglGetProcAddress("glUniform2fARB");
	glUniform1iARB = (PFNGLUNIFORM1IARBPROC)wglGetProcAddress("glUniform1iARB");
	glUniformMatrix4fv = (PFNGLUNIFORMMATRIX4FVPROC)wglGetProcAddress("glUniformMatrix4fv");
	glGetUniformLocationARB = (PFNGLGETUNIFORMLOCATIONARBPROC)wglGetProcAddress("glGetUniformLocationARB");
	

	glActiveTexture = (PFNGLACTIVETEXTUREPROC)wglGetProcAddress("glActiveTexture");

	init = true;
}

Shader::Shader()
{
	//if (!init)
		//initShadersFunctions();
}

void Shader::LoadShaderFromFile()
{
	if (!init)
		initShadersFunctions();

	glDeleteObjectARB(program);
	glDeleteObjectARB(vertex);
	glDeleteObjectARB(fragment);

	program = glCreateProgramObjectARB();
	vertex = glCreateShaderObjectARB(GL_VERTEX_SHADER_ARB);
	fragment = glCreateShaderObjectARB(GL_FRAGMENT_SHADER_ARB);


	char *shader1Text;
	char *shader2Text;
	int shader1TextSize;
	int shader2TextSize;

	loadShader(FshaderFileName, &shader1Text, &shader1TextSize);
	loadShader(VshaderFileName, &shader2Text, &shader2TextSize);

	const char *_f = shader1Text;
	const char *_v = shader2Text;

	glShaderSourceARB(fragment, 1, &_f, &shader1TextSize);
	glShaderSourceARB(vertex, 1, &_v, &shader2TextSize);


	free(shader1Text);
	free(shader2Text);
}

int Shader::loadPixShader(const char ** text, int*lengths,int count, char ** err)
{
	if (!init)
		initShadersFunctions();

	glDeleteObjectARB(fragment);
	fragment = glCreateShaderObjectARB(GL_FRAGMENT_SHADER_ARB);
	
	glShaderSourceARB(fragment, count, text, lengths);

	return 1;
}

int Shader::loadVertShader(const char** text, int* lengths, int count, char** err)
{
	if (!init)
		initShadersFunctions();
	glDeleteObjectARB(vertex);
	
	vertex = glCreateShaderObjectARB(GL_VERTEX_SHADER_ARB);

	glShaderSourceARB(vertex, count, text, lengths);

	return 1;
}

void Shader::Compile()
{
	compiled = true;
	//компиляция фрагментного шейдера
	errStr.clear();
	
	    glCompileShaderARB(fragment);
		
		{
			//Получение сообщения о ошибках компиляции
			int compiled = 0; int length = 0; int laux = 0;
			glGetObjectParameterivARB(fragment, GL_OBJECT_COMPILE_STATUS_ARB, &compiled);
			glGetObjectParameterivARB(fragment, GL_OBJECT_INFO_LOG_LENGTH_ARB, &length);
			char *log = new char[length];
			glGetInfoLogARB(fragment, length, &laux, log);
			wchar_t *_log = new wchar_t[length];
			mbstowcs(_log, log, length);
			
			if (!compiled)
			{
				//MessageBox(0, _log, L"fragment shader error!", 0);
				compiled = false;
				errStr.append("Fragment shader error:\n");
				errStr.append(log);
				errStr.append("=====\n");
			}
				
			delete _log;
			delete log;
		}
		
		
		glCompileShaderARB(vertex);
		
		{
			int compiled = 0; int length = 0; int laux = 0;

			glGetObjectParameterivARB(vertex, GL_OBJECT_COMPILE_STATUS_ARB, &compiled);
			glGetObjectParameterivARB(vertex, GL_OBJECT_INFO_LOG_LENGTH_ARB, &length);
			char *log = new char[length];
			glGetInfoLogARB(vertex, length, &laux, log);
			wchar_t *_log = new wchar_t[length];
			mbstowcs(_log, log, length);//
			if (!compiled)
			{
				//MessageBox(0, _log, L"vertex shader error!", 0);
				compiled = false;
				errStr.append("Vertex shader error:\n");
				errStr.append(log);
				errStr.append("=====\n");
			}
				
			delete log;
			delete _log;
		}
		
		//привязывем шейдер к программному объекту
		glDeleteObjectARB(program);
		program = glCreateProgramObjectARB();
		
		glAttachObjectARB(program, fragment);
		glAttachObjectARB(program, vertex);
		
		//линкуем
		glLinkProgramARB(program);
		
		{
			int result = 0; int length = 0; int laux = 0;
			glGetObjectParameterivARB(program, GL_OBJECT_LINK_STATUS_ARB, &result);
			glGetObjectParameterivARB(program, GL_OBJECT_INFO_LOG_LENGTH_ARB, &length);
			char *log = new char[length];
			glGetInfoLogARB(program, length, &laux, log);
			wchar_t *_log = new wchar_t[length];
			mbstowcs(_log, log, length);//
			if (!result)
			{
				//MessageBox(0, _log, L"shader error!", 0);
				compiled = false;
				errStr.append("Shader compile error:\n");
				errStr.append(log);
				errStr.append("=====\n");
			}
			
			delete _log;
			delete log;
		}
		
		

	
}

void Shader::UseShader()
{
	if (compiled)
		glUseProgramObjectARB(program);	
}

void Shader::DontUseShaders()
{
	glUseProgramObjectARB(0);	
}
