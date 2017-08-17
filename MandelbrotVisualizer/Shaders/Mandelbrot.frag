#version 410

in vec4 passPosition;
out vec4 fragColor;
uniform dvec2 centerPoint;
uniform double unitSize;
uniform double aspectRatio;

int maxIteration = 100;
double bailoutRadius = 2.0;

vec4 GetFragColor(){
	dvec2 unitPosition = dvec2(passPosition.xy / passPosition.w); //[-1, 1]
	dvec2 point = vec2(unitPosition.x * aspectRatio * unitSize, unitPosition.y * unitSize) + centerPoint;
	
	double x = 0.0;
	double y = 0.0;
	int iteration = 0;
	while (x*x + y*y < bailoutRadius*bailoutRadius  &&  iteration < maxIteration) {
		double xtemp = x*x - y*y + point.x;
		y = 2*x*y + point.y;
		x = xtemp;
		++iteration;
	}
	
	//coloring
	float whiteValue = 1.0 - float(iteration) / float(maxIteration);
	vec4 color = vec4(whiteValue, whiteValue, whiteValue, 1.0);
	return color;
}

void main(void){
    //fragColor = vec4(0.0, 0.0, 1.0, 1.0);
    fragColor = GetFragColor();
}