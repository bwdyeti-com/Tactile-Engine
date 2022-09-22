float4x4 World : WORLD;
float4x4 View;
float4x4 Projection;

//float2   ViewportSize    : register(c0);
//float2   TextureSize     : register(c1);
//float4x4 MatrixTransform : register(c2);

sampler  TextureSampler  : register(s0);
sampler	 Map_Alpha       : register(s1);
sampler  Palette         : register(s2);

float4x4 MatrixTransform;

uniform float4 tone;
uniform float4 color_shift;
uniform float opacity;
uniform float2 alpha_offset;
uniform float2 map_size;
uniform float2 game_size;

//--------------------------------------------------------------------------------
// Vertex Shader
//--------------------------------------------------------------------------------
struct VertexOutput
{
    float4 position : POSITION0;
    float4 color: COLOR0;
	float2 uv : TEXCOORD0;
    float4 palette_color: COLOR1;
};

VertexOutput original_vs(float4 position : POSITION0, float4 color    : COLOR0, float2 texCoord : TEXCOORD0)
{
	VertexOutput output = (VertexOutput)0;
	
    float4 worldPosition = mul(position, World);
    float4 viewPosition = mul(worldPosition, View);
    output.position = mul(viewPosition, Projection);

	output.position = mul(output.position, MatrixTransform);

	output.color = color;
	output.uv = texCoord;
	output.palette_color = float4(1, 0, 0, 0);

    return output;
}

VertexOutput palette_change(float4 position : POSITION0, float4 color : COLOR0, float2 texCoord : TEXCOORD0)
{
	VertexOutput output = (VertexOutput)0;
	
    float4 worldPosition = mul(position, World);
    float4 viewPosition = mul(worldPosition, View);
	output.position = mul(viewPosition, Projection);

	output.position = mul(output.position, MatrixTransform);

	output.color = color;
	output.uv = texCoord;
	//output.palette_color = float4(texCoord.x, texCoord.y, 0, 1);
	//output.palette_color = tex2D(Palette, output.uv);
	//output.palette_color = tex2D(Map_Alpha, float2(0.5, 0.5));
	output.palette_color.a = 1;

	return output;


    /* ... */
    //float distance = length((in_Position - in_ShipCenter).xyz);
    //float time = in_Time;

    /* Simple distance/time combination */
    float2 colorIndex = float2(0.5, 0.5);
    //float2 colorIndex = float2(distance + time, 0.5);

    /* Other possible effects:
     * vec2 colorIndex = vec2(A * distance + B * time, 0.5);
     * vec2 colorIndex = vec2(distance, time);
     * vec2 colorIndex = vec2(distance + A * time, time + B * distance); */

    /* Query the colour in the two-dimensional texture palette. */
    //output.color = tex1D(Palette, colorIndex);

	return output;
}

//--------------------------------------------------------------------------------
// Pixel Shader
//--------------------------------------------------------------------------------
struct PixelInput
{
    float4 color: COLOR0;
	float2 uv : TEXCOORD0;
    float4 palette_color: COLOR1;
};

float4 main_palette(PixelInput input) : COLOR
{
    // Look up the texture color.
	float4 Color = tex2D(TextureSampler, input.uv);

	float4 PaletteColor = tex2D(Palette, Color.r);
	if (Color.a == 0)
		Color.rgb = PaletteColor.rgb;
	else
		Color = PaletteColor;

	Color.rgb *= 1.0f - color_shift.a;
	Color.rgb += (color_shift.rgb * color_shift.a);
	Color.rgb *= Color.a;

	/*
	//Color.rgb *= 1.0f - color_shift.a;
	//Color.rgb += (color_shift.rgb * color_shift.a) * Color.a;
	*/
    return Color * input.color * opacity;
}

float4 tone_palette(PixelInput input) : COLOR
{
    // Look up the texture color.
    float4 Color = tex2D(TextureSampler, input.uv);

	Color.rgb = tex2D(Palette, Color.r).rgb;

	float3 grey_color = dot(Color.rgb, float3(0.3, 0.59, 0.11));
	//Color.rgb = (Color.rgb * (1.0f - tone.a)) + ((grey_color.rgb * tone.a) + tone.rgb) * Color.a; //Debug
	// This was different as a non-lerp, the 'tone.rgb * Color.a' was applied to grey_color.rgb, is this because of opacity issues? //Yeti
	// Look into sprites with non-opaque colors in their palette and see what happens
	grey_color *= Color.a; // With this line added it should be fine though; just try a 100% grey tone and see what happens
	Color.rgb = lerp(Color.rgb, grey_color.rgb, tone.a) + tone.rgb * Color.a;

    return Color * input.color * opacity;
}

float4 main(PixelInput input) : COLOR
{
    // Look up the texture color.
    float4 Color = tex2D(TextureSampler, input.uv);
	if (input.palette_color.a > 0)
	{
		//Color.rgb = tex2D(Palette, Color.r).rgb;
		//Color.rgb = input.palette_color.rbg; //testing to see if this does anything
	}

	Color.rgb *= 1.0f - color_shift.a;
	Color.rgb += (color_shift.rgb * color_shift.a);
	Color.rgb *= Color.a;

    return Color * input.color * opacity;
}

float4 effect(float4 color : COLOR0, float2 uv : TEXCOORD0) : COLOR
{
    // Look up the texture color.
    float4 Color = tex2D(TextureSampler, uv);
	Color.rgb *= 1.0f - color_shift.a;
	Color.rgb += (color_shift.rgb * color_shift.a) * Color.a;

    return Color * color;
}

float4 tone_effect(float4 color : COLOR0, float2 uv : TEXCOORD0) : COLOR
{
    // Look up the texture color.
    float4 Color = tex2D(TextureSampler, uv);
	// Converts original color to greyscale
	float3 grey_color = dot(Color.rgb, float3(0.3, 0.59, 0.11));
	//Color.rgb = (Color.rgb * (1.0f - tone.a)) + (grey_color.rgb * tone.a) + tone.rgb * Color.a; // Oh this is just a lerp, huh //Debug
	Color.rgb = lerp(Color.rgb, grey_color.rgb, tone.a) + tone.rgb * Color.a;

    return Color * color;
}

//--------------------------------------------------------------------------------
// Effects
//--------------------------------------------------------------------------------
technique Technique1
{
    pass Pass1
    {
        VertexShader = compile vs_2_0 original_vs();
        PixelShader  = compile ps_2_0 main();
    }
}

technique Technique2
{
    pass Pass1
    {
        VertexShader = compile vs_2_0 original_vs();
        PixelShader  = compile ps_2_0 effect();
    }
}

technique Palette1
{
    pass Pass1
    {
        //VertexShader = compile vs_2_0 palette_change();
        VertexShader = compile vs_2_0 original_vs();
        PixelShader  = compile ps_2_0 main_palette();
    }
}

technique Palette2
{
    pass Pass1
    {
        VertexShader = compile vs_2_0 original_vs();
        PixelShader  = compile ps_2_0 tone_palette();
    }
}

technique Tone
{
    pass Pass1
    {
        VertexShader = compile vs_2_0 original_vs();
        PixelShader  = compile ps_2_0 tone_effect();
    }
}

//-----------------------------------------------------------------------------
// Default Render
//-----------------------------------------------------------------------------

float4 original_ps(float4 color : COLOR0, float2 uv : TEXCOORD0) : COLOR
{
	float4 Color = tex2D(TextureSampler, uv);
	return Color * color;
}

technique Normal
{
	pass Pass1
    {
        VertexShader = compile vs_2_0 original_vs();
        PixelShader  = compile ps_2_0 original_ps();
    }
}





//-----------------------------------------------------------------------------
// Map Lighting
//-----------------------------------------------------------------------------

float4 map_lighting(float4 color : COLOR0, float2 uv : TEXCOORD0) : COLOR
{
	float4 Color = tex2D(TextureSampler, uv);
	Color *= tex2D(Map_Alpha, (uv * game_size + alpha_offset) / (16 * map_size));
	return Color * color;
}

technique Map_Lighting
{
	pass Pass1
	{
		VertexShader = compile vs_2_0 original_vs();
		PixelShader = compile ps_2_0 map_lighting();
	}
}

//-----------------------------------------------------------------------------
// Mask
//-----------------------------------------------------------------------------

uniform float4 mask_rect;
uniform float2 mask_size_ratio;

// Masks texture with a rectangular section of 'Map_Alpha'
float4 mask(float4 color : COLOR0, float2 uv : TEXCOORD0) : COLOR
{
	float4 Color = tex2D(TextureSampler, uv);
	uv /= mask_size_ratio;
	uv += alpha_offset;

	float2 mask_uv = uv;

	if (mask_uv.x <= mask_rect[2] && mask_uv.y <= mask_rect[3] && mask_uv.x >= mask_rect[0] && mask_uv.y >= mask_rect[1])
	{
		Color *= tex2D(Map_Alpha, mask_uv).a;
	}
	else
		Color *= 0;
	return Color * color;
}

technique Mask
{
	pass Pass1
	{
		VertexShader = compile vs_2_0 original_vs();
		PixelShader = compile ps_2_0 mask();
	}
}

//-----------------------------------------------------------------------------
// Minimap Mask
//-----------------------------------------------------------------------------

uniform float minimap_scale;
uniform float minimap_angle;

// Masks out rectangular section of the minimap, (and also applies screen tone to it? this doesn't seem to happen)
float4 minimap_mask(float4 color : COLOR0, float2 uv : TEXCOORD0) : COLOR
{
	float4 Color = tex2D(TextureSampler, uv);
	uv -= 0.5f;
	uv.x *= game_size.x;
	uv.y *= game_size.y;
	uv *= minimap_scale;
	uv /= 256;

	float2 mask_uv;
	float ca = cos(minimap_angle);
	float sa = sin(minimap_angle);
	mask_uv.x = uv.x * ca - uv.y * sa;
	mask_uv.y = uv.x * sa + uv.y * ca;
	mask_uv /= map_size / 64;
	mask_uv += 0.5f;

	if (mask_uv.x <= 1 && mask_uv.y <= 1 && mask_uv.x >= 0 && mask_uv.y >= 0)
	{
		Color *= tex2D(Map_Alpha, mask_uv);
	}
	else
		Color *= 0;
	return Color * color;
}

technique Minimap_Mask
{
	pass Pass1
	{
		VertexShader = compile vs_2_0 original_vs();
		PixelShader = compile ps_2_0 minimap_mask();
	}
}

//-----------------------------------------------------------------------------
// Outline Glow
//-----------------------------------------------------------------------------

// Applies 'color_shift' to all pixels that resolve to 'tone'
float4 outline_glow(float4 color : COLOR0, float2 uv : TEXCOORD0) : COLOR
{
	float4 Color = tex2D(TextureSampler, uv);
	if (Color.r == tone.r && Color.g == tone.g && Color.b == tone.b)
	{
		//Color.rgb = (Color.rgb * (1.0f - color_shift.a)) + (color_shift.rgb * color_shift.a); //Debug
		Color.rgb = lerp(Color.rgb, color_shift.rgb, color_shift.a);
	}
	return Color * color;
}

technique Outline_Glow
{
	pass Pass1
	{
		VertexShader = compile vs_2_0 original_vs();
		PixelShader = compile ps_2_0 outline_glow();
	}
}

//-----------------------------------------------------------------------------
// Distortion
//-----------------------------------------------------------------------------

// The Distortion map represents zero displacement as 0.5, but in an 8 bit color
// channel there is no exact value for 0.5. ZeroOffset adjusts for this error.
static const float ZeroOffset = 0.5f / 255.0f;

#define SAMPLE_COUNT 13 // 15 // Must be odd!
float2 SampleOffsets[SAMPLE_COUNT];
float SampleWeights[SAMPLE_COUNT];

// Distortion texture with a rectangular section of 'Map_Alpha'
float4 distortion(float4 color : COLOR0, float2 uv : TEXCOORD0) : COLOR
{
	//float2 mask_uv = uv; //Debug
	//mask_uv /= mask_size_ratio;
	//mask_uv += alpha_offset;

	float4 Color;
	//if (mask_uv.x <= mask_rect[2] && mask_uv.y <= mask_rect[3] && mask_uv.x >= mask_rect[0] && mask_uv.y >= mask_rect[1]) //Debug
	//{
	// Look up the displacement
		float4 displacement = tex2D(Map_Alpha, uv); // mask_uv); //Debug

		if (displacement.a > 0)
		{
			displacement.rgb /= displacement.a;
			// Convert from [0,1] to [-.5, .5) 
			// .5 is excluded by adjustment for zero
			displacement.rg -= .5 + ZeroOffset;
			displacement.rg *= 0.75; // This is a maximum range of 120 pixels
			//displacement.rg *= displacement.a / 2; // This is a maximum range of 80 pixels
			displacement.rg /= mask_size_ratio;

			if (false)
			{
				displacement.rg *= displacement.a;
				Color = 0;
				// Combine a number of weighted displaced-image filter taps
				for (int i = 0; i < SAMPLE_COUNT; i++)
				{
					Color += tex2D(TextureSampler, uv.xy + displacement.rg +
						SampleOffsets[i]) * SampleWeights[i];
				}
			}
			else
			{
				// Get a combination of the original color and the displaced color
				Color = tex2D(TextureSampler, uv);
				Color = lerp(Color, tex2D(TextureSampler, uv + displacement.rg), displacement.a);
				// Get displaced color, using a reduced range based on displacement.a
				displacement.rg *= displacement.a;
				float4 target_color = tex2D(TextureSampler, uv + displacement.rg);
				// Lerp between the two colors using displacement.b
				Color = lerp(target_color, Color, displacement.b);
			}
		}
		else
			Color = tex2D(TextureSampler, uv);
	//}

	return Color * color;
}

technique Distortion
{
	pass Pass1
	{
		VertexShader = compile vs_2_0 original_vs();
		PixelShader = compile ps_2_0 distortion();
	}
}

uniform float2 distortion_flip;

// Corrects for distortion mirroring
float4 distortion_flip_correction(float4 color : COLOR0, float2 uv : TEXCOORD0) : COLOR
{
	float4 Color = tex2D(TextureSampler, uv);
	// Flip red/green if needed
	if (distortion_flip.x < 0)
		Color.r = lerp(1 / 255.0f, 256 / 255.0f, Color.a - Color.r);
	if (distortion_flip.y < 0)
		Color.g = lerp(1 / 255.0f, 256 / 255.0f, Color.a - Color.g);
	// Multiply in tint
	Color *= color;
	// Lerp with vector representing no distortion?
	//float4 no_distortion = float4(0.5 + ZeroOffset, 0.5 + ZeroOffset, 0, 1);
	//Color = lerp(no_distortion, Color, color.a);
	return Color;
}

technique DistortionFlip
{
	pass Pass1
	{
		VertexShader = compile vs_2_0 original_vs();
		PixelShader = compile ps_2_0 distortion_flip_correction();
	}
}

//-----------------------------------------------------------------------------
// Color Deficiency
//-----------------------------------------------------------------------------

void RGB_to_LMS(float3 Color, out float3 L, out float3 M, out float3 S)
{
	// RGB to LMS matrix conversion
	L = (17.8824f * Color.r) + (43.5161f * Color.g) + (4.11935f * Color.b);
	M = (3.45565f * Color.r) + (27.1554f * Color.g) + (3.86714f * Color.b);
	S = (0.0299566f * Color.r) + (0.184309f * Color.g) + (1.46709f * Color.b);
}

float4 LMS_to_RGB(float l, float m, float s, float a)
{
	// LMS to RGB matrix conversion
	float4 error;
	error.r = (0.0809444479f * l) + (-0.130504409f * m) + (0.116721066f * s);
	error.g = (-0.0102485335f * l) + (0.0540193266f * m) + (-0.113614708f * s);
	error.b = (-0.000365296938f * l) + (-0.00412161469f * m) + (0.693511405f * s);
	error.a = a;

	return error;
}

// Modifies colors to simulate common color deficiencies
float4 protanopia(float4 color : COLOR0, float2 uv : TEXCOORD0) : COLOR
{
	float4 Color = tex2D(TextureSampler, uv);
	float3 L, M, S;
	RGB_to_LMS(Color.rgb, L, M, S);

	// Simulate color blindness
	// Protanope - reds are greatly reduced (1% men)
	float l = 0.0f * L + 2.02344f * M + -2.52581f * S;
	float m = 0.0f * L + 1.0f * M + 0.0f * S;
	float s = 0.0f * L + 0.0f * M + 1.0f * S;

	float4 error = LMS_to_RGB(l, m, s, Color.a);
	return error * color;
}

technique Protanopia
{
	pass Pass1
	{
		VertexShader = compile vs_2_0 original_vs();
		PixelShader = compile ps_2_0 protanopia();
	}
}

// Modifies colors to simulate common color deficiencies
float4 deuteranopia(float4 color : COLOR0, float2 uv : TEXCOORD0) : COLOR
{
	float4 Color = tex2D(TextureSampler, uv);
	float3 L, M, S;
	RGB_to_LMS(Color.rgb, L, M, S);

	// Simulate color blindness
	// Deuteranope - greens are greatly reduced (1% men)
	float l = 1.0f * L + 0.0f * M + 0.0f * S;
	float m = 0.494207f * L + 0.0f * M + 1.24827f * S;
	float s = 0.0f * L + 0.0f * M + 1.0f * S;

	float4 error = LMS_to_RGB(l, m, s, Color.a);
	return error * color;
}

technique Deuteranopia
{
	pass Pass1
	{
		VertexShader = compile vs_2_0 original_vs();
		PixelShader = compile ps_2_0 deuteranopia();
	}
}

// Modifies colors to simulate common color deficiencies
float4 tritanopia(float4 color : COLOR0, float2 uv : TEXCOORD0) : COLOR
{
	float4 Color = tex2D(TextureSampler, uv);
	float3 L, M, S;
	RGB_to_LMS(Color.rgb, L, M, S);

	// Simulate color blindness
	// Tritanope - blues are greatly reduced (0.003% population)
	float l = 1.0f * L + 0.0f * M + 0.0f * S;
	float m = 0.0f * L + 1.0f * M + 0.0f * S;
	float s = -0.395913f * L + 0.801109f * M + 0.0f * S;

	float4 error = LMS_to_RGB(l, m, s, Color.a);
	return error * color;
}

technique Tritanopia
{
	pass Pass1
	{
		VertexShader = compile vs_2_0 original_vs();
		PixelShader = compile ps_2_0 tritanopia();
	}
}

//-----------------------------------------------------------------------------
// Coverage Upscaling Shader
//-----------------------------------------------------------------------------

uniform float2 display_scale;

// Better interpolation for upscaling pixels; effectively scales up infinitely with nearest neighbor, then downscales with linear
float4 coverage_shader(float4 color : COLOR0, float2 uv : TEXCOORD0) : COLOR
{
	// Represents the size of one pixel
	float3 step = float3(1.0 / display_scale.x, 1.0 / display_scale.y, 0);
	//float2 tex_pixel = game_size * Input.TexCoord.xy - step.xy / 2; //Debug
	float2 tex_pixel = game_size * uv - step.xy / 2;

	float2 corner = floor(tex_pixel) + 1;
	float2 frac = min((corner - tex_pixel) * display_scale, float2(1.0, 1.0));

	float4 c1 = tex2D(TextureSampler, (floor(tex_pixel + step.zz) + 0.5) / game_size);
	float4 c2 = tex2D(TextureSampler, (floor(tex_pixel + step.xz) + 0.5) / game_size);
	float4 c3 = tex2D(TextureSampler, (floor(tex_pixel + step.zy) + 0.5) / game_size);
	float4 c4 = tex2D(TextureSampler, (floor(tex_pixel + step.xy) + 0.5) / game_size);

	c1 *= frac.x  *        frac.y;
	c2 *= (1.0 - frac.x) *        frac.y;
	c3 *= frac.x  * (1.0 - frac.y);
	c4 *= (1.0 - frac.x) * (1.0 - frac.y);

	return (c1 + c2 + c3 + c4) * color;
}

technique Coverage_Shader
{
	pass Pass1
	{
		VertexShader = compile vs_2_0 original_vs();
		PixelShader = compile ps_2_0 coverage_shader();
	}
}