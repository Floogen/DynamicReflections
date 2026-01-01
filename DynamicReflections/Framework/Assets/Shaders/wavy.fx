#if OPENGL
    #define SV_POSITION POSITION
    #define VS_SHADERMODEL vs_3_0
    #define PS_SHADERMODEL ps_3_0
#else
    #define VS_SHADERMODEL vs_4_0_level_9_1
    #define PS_SHADERMODEL ps_4_0_level_9_1
#endif

float percent;

float4 ColorOverlay;
float Frequency = 100;
float Phase = 0;
float Amplitude = 0.1;

float2 RenderDimensions;
float2 ViewportCoordinates;


sampler2D TextureSampler : register(s0)
{
    Texture = (Texture);
};

float4 WavyPS(float4 position : SV_Position, float4 color : COLOR0, float2 TextureCoordinates : TEXCOORD0) : COLOR0
{
    float2 uv = TextureCoordinates;
    uv.x += sin(uv.y * Frequency + Phase) * Amplitude;
    
    return tex2D(TextureSampler, uv) * ColorOverlay;
}

float4 WavyDarkPS(float4 position : SV_Position, float4 color : COLOR0, float2 TextureCoordinates : TEXCOORD0) : COLOR0
{
    // https://community.monogame.net/t/how-to-use-a-pixel-shader-with-spritebatch-minimal-example-greyscale/12132
    //float4 col = tex2D(TextureSampler, TextureCoordinates)* color;
    //col.rgb = (col.r + col.g + col.b) / 3.0f * percent; // grey scale and darken
    //return col;
    
    // https://vvvv.org/documentation/tutorial-effects-texture-coordinates
    //float2 cord = TextureCoordinates;
    //cord.x += sin(cord.y * Frequency + Phase) * Amplitude;
    //float4 col = tex2D(TextureSampler, cord) * color;
    //return col;
    
    float2 uv = TextureCoordinates;
    uv.x += sin(uv.y * Frequency + Phase) * Amplitude;

    float4 col = tex2D(TextureSampler, uv) * color;
    col.rgb = (col.r + col.g + col.b) / 3.0f * percent; // grey scale and darken
    return col;
}


float4 WavyWorldPositionPS(float4 position : SV_Position, float4 color : COLOR0, float2 TextureCoordinates : TEXCOORD0) : COLOR0
{
    float2 uv = TextureCoordinates;
    
    float2 worldPosition = (position.xy + ViewportCoordinates) / RenderDimensions;
    uv.x += sin(worldPosition.y * Frequency + Phase) * Amplitude;
    
    return tex2D(TextureSampler, uv) * ColorOverlay;
}

technique Wavy
{
    pass P0
    {
        PixelShader = compile PS_SHADERMODEL WavyPS();
    }
};

technique WavyWorldPosition
{
    pass P0
    {
        PixelShader = compile PS_SHADERMODEL WavyWorldPositionPS();
    }
};

technique WavyDark
{
    pass P0
    {
        PixelShader = compile PS_SHADERMODEL WavyPS();
    }
    pass P1
    {
        PixelShader = compile PS_SHADERMODEL WavyDarkPS();
    }
};