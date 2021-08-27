#include "pch.h"
#include "Shader.h"
#include <vector>

Shader::Shader( const char *VertPath, const char *FragPath )
{
	std::string VertCode;
	std::string FragCode;
	std::ifstream VShaderFile;
	std::ifstream FShaderFile;

	VShaderFile.exceptions( std::ifstream::failbit | std::ifstream::badbit );
	FShaderFile.exceptions( std::ifstream::failbit | std::ifstream::badbit );

	try
	{
		// open files
		VShaderFile.open( VertPath );
		FShaderFile.open( FragPath );
		std::stringstream VShaderStream, FShaderStream;
		// read file's buffer contents into streams
		VShaderStream << VShaderFile.rdbuf();
		FShaderStream << FShaderFile.rdbuf();
		// close file handlers
		VShaderFile.close();
		FShaderFile.close();
		// convert stream into string
		VertCode = VShaderStream.str();
		FragCode = FShaderStream.str();
	}
	catch ( const std::exception &e )
	{
		std::cout << e.what() << std::endl;
	}

	const char *VShaderCode = VertCode.c_str();
	const char *FShaderCode = FragCode.c_str();

	unsigned int Vertex, Fragment;
	int success;
	char infoLog[ 512 ];

	// vertex Shader
	Vertex = glCreateShader( GL_VERTEX_SHADER );
	glShaderSource( Vertex, 1, &VShaderCode, NULL );
	glCompileShader( Vertex );
	// print compile errors if any
	glGetShaderiv( Vertex, GL_COMPILE_STATUS, &success );
	if ( !success )
	{
		glGetShaderInfoLog( Vertex, 512, NULL, infoLog );
		std::cout << "ERROR::SHADER::VERTEX::COMPILATION_FAILED\n" << infoLog << std::endl;
	};

	// similiar for Fragment Shader
	Fragment = glCreateShader( GL_FRAGMENT_SHADER );
	glShaderSource( Fragment, 1, &FShaderCode, NULL );
	glCompileShader( Fragment );
	// print compile errors if any
	glGetShaderiv( Fragment, GL_COMPILE_STATUS, &success );
	if ( !success )
	{
		glGetShaderInfoLog( Vertex, 512, NULL, infoLog );
		std::cout << "ERROR::SHADER::VERTEX::COMPILATION_FAILED\n" << infoLog << std::endl;
	};

	// shader Program
	this->ID = glCreateProgram();
	glAttachShader( ID, Vertex );
	glAttachShader( ID, Fragment );
	glLinkProgram( ID );
	// print linking errors if any
	glGetProgramiv( ID, GL_LINK_STATUS, &success );
	if ( !success )
	{
		glGetProgramInfoLog( ID, 512, NULL, infoLog );
		std::cout << "ERROR::SHADER::PROGRAM::LINKING_FAILED\n" << infoLog << std::endl;
	}

	// delete the shaders as they're linked into our program now and no longer necessary
	glDeleteShader( Vertex );
	glDeleteShader( Fragment );
}

Shader::Shader()
{
}

void InitShader( const char *VertPath, const char *FragPath, Shader *pShader )
{
	_ASSERTE( pShader );
	*pShader = Shader( VertPath, FragPath );
}

void UseShader( Shader s )
{
	glUseProgram( s.ID );
}

void SetBool( Shader s, const std::string &name, bool value )
{ 
	glUniform1i( glGetUniformLocation( s.ID, name.c_str() ), (int) value ); 
}
void SetInt( Shader s, const std::string &name, int value )
{ 
	glUniform1i( glGetUniformLocation( s.ID, name.c_str() ), value );
}
void SetFloat( Shader s, const std::string &name, float value )
{ 
	glUniform1f( glGetUniformLocation( s.ID, name.c_str() ), value );
}
void SetMatrix( Shader s, const std::string &name, glm::mat4 value )
{
	glUniformMatrix4fv( glGetUniformLocation( s.ID, name.c_str() ), 1, GL_FALSE, glm::value_ptr( value ) );
}
void SetVec4Array( Shader s, const std::string &name, std::vector<glm::vec4> values )
{
	std::vector<float> floatvalues;
	for ( int i = 0; i < 100; ++i )
	{
		for ( int j = 0; j < 4; ++j )
		{
			if ( i < values.size() )
				floatvalues.push_back( values[ i ][ j ] );
			else
				floatvalues.push_back( 0.0f );
		}
	}
	_ASSERTE( floatvalues.size() == 400 );
	glUniform4fv( glGetUniformLocation( s.ID, name.c_str() ), 100, floatvalues.data() );
}
void SetFloatArray( Shader s, const std::string &name, std::vector<float> values )
{
	if ( values.size() < 100 )
	{
		for ( int i = values.size(); i < 100; ++i )
		{
			values.push_back( 0.0f );
		}
	}
	_ASSERTE( values.size() == 100 );
	glUniform1fv( glGetUniformLocation( s.ID, name.c_str() ), 100, values.data() );
}