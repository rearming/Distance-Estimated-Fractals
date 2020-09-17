Shader "Core/Raymarching"
{
    Properties
    {
        _MaxRaySteps("Maximum Ray Steps", Int) = 50
        _MinimumDistance("Minimum Distance", float) = 0.1
    	
    	[Header(Mandelbulb Settings)]
    	
    	_Bailout ("Bailout", float) = 0.01
    	_Power ("Power", float) = 10
    	_MaxIterations ("Max Iterations", Int) = 50 
    }
    SubShader
    {
        // No culling or depth
        Cull Off ZWrite Off ZTest Always

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }
            
            int _MaxRaySteps;
            float _MinimumDistance;
            
            float4x4 _CamRotationMatrix;

            float _Bailout;
            float _Power;
            int _MaxIterations;

			float mandelbulbDE(float3 pos)
            {
				float3 z = pos;
				float dr = 1.0;
				float r = 0.0;
				for (int i = 0; i < _MaxIterations; i++)
				{
					r = length(z);
					if (r > _Bailout)
						break;
					
					// convert to polar coordinates
					float theta = acos(z.z / r);
					float phi = atan2(z.y, z.x);
					dr =  pow(r, _Power - 1.0) * _Power * dr + 1.0;
					
					// scale and rotate the point
					float zr = pow(r, _Power);
					theta = theta * _Power;
					phi = phi * _Power;
					
					// convert back to cartesian coordinates
					z = zr * float3(sin(theta) * cos(phi), sin(phi) * sin(theta), cos(theta));
					z += pos;
				}
				return 0.5 * log(r) * r / dr;
			}

            float sphereDE(float3 p)
            {
	            return length(p) - 1.0;
            }
            
            float DistanceEstimator(float3 p)
            {
	            // return sphereDE(p);
				return mandelbulbDE(p);
            }
            
            float trace(float3 from, float3 direction)
            {
                float totalDistance = 0.0;
                int steps;
                for (steps = 0; steps < _MaxRaySteps; steps++)
                {
                    float3 p = from + totalDistance * direction;
		            float distance = DistanceEstimator(p);
		            totalDistance += distance;
		            if (distance < _MinimumDistance)
		                break;
	            }
	            return 1.0 - float(steps) / float(_MaxRaySteps);
            }
            
            fixed4 frag (v2f i) : SV_Target
            {
                float3 rayOrigin = _WorldSpaceCameraPos;
                float3 rayDir = mul(_CamRotationMatrix, float4((i.uv - 0.5) * 2, 1, 1));

                float grayscale = trace(rayOrigin, rayDir);
                return fixed4(grayscale, grayscale, grayscale, 1);
            }
            ENDCG
        }
    }
}
