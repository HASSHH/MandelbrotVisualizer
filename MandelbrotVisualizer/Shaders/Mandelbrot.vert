#version 410

in vec4 inPosition;
out vec4 passPosition;
uniform mat4 mvpMatrix;

void main(void){
	vec4 pos = mvpMatrix*inPosition;

	passPosition = pos;
    gl_Position = pos;
}