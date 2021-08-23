# CSharpPhysEngine
A physics engine i'm writing in c# that uses a render.dll i built in c++ that uses opengl.

The "must write to gl_position" error shows up in console when the program can't find the shaders. Make sure to either place the shaders in a folder titled Shaders/ in the executable directory or change the file path in mainclass::Init() (mainclass.cpp, line 74).
