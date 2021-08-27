#version 330 core
layout (location = 0) in vec3 aPos;
layout (location = 1) in vec2 aTexCoord;

uniform mat4 transform;
uniform mat4 CameraTransform;
uniform mat4 Perspective;

void main()
{
    gl_Position = vec4( aPos, 1.0 );
}