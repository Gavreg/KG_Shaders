#ifndef TEXTURE_H
#define TEXTURE_H

#include <string>
#include <windows.h>
#include <GL\GL.h>

class Texture
{
public: 
	~Texture();

	unsigned int texId = 0;

	void loadTextureFromFile(LPWSTR filename);
	void loadFromBuffer(unsigned char *buf, int w, int h);
	void deleteTexture();

	void bindTexture();
	
	static int Texture::LoadBMP(__in LPWSTR  filename, __out int* Wigth, __out int *Height, __out RGBTRIPLE **arr);
	static int RGBtoChar(__in RGBTRIPLE *arr, __in int width, __in int height, __out char **out);

};


#endif //TEXTURE_H