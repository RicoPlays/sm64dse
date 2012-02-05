
uniform vec2 m_CamRotation;

void main(void)
{
	float rx = gl_Vertex.x * cos(-m_CamRotation.y) * cos(-m_CamRotation.x);
	float ry = gl_Vertex.y * sin(-m_CamRotation.y);
	float rz = gl_Vertex.z * con(-m_CamRotation.y) * sin(-m_CamRotation.x);
	
	gl_Position = vec3(rx, ry, rz);
}