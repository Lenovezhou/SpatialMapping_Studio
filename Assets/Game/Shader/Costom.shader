Shader "Custom/OcclusionView" 
{
	Properties
	{
		_MainTex("MainTex_Base",2D) = "white"{}
	    _RimColor("RimColor",Color) = (0,1,1,1)
		_RimPower("RimPower",Range(0.1,8.0)) = 1.0
	}

	 SubShader
	{
		LOD 200
		Tags{ "Queue" = "Geometry+500" "RenderType" = "Opaque" }
	   Pass
	  {
		//混合遮挡物和被遮挡物的颜色
		Blend SrcAlpha One

		//不记录像素的深度值
		ZWrite off
		Lighting off
		Ztest greater

		//渲染队列走到渲染该物体时，深度值大于之前渲染物体深度最小值时，便在物体后面执行渲染

		CGPROGRAM
        #pragma vertex vert
        #pragma fragment frag
        #include "UnityCG.cginc"
        #include "Lighting.cginc"

		fixed4 _RimColor;
	    float _RimPower;
	    
	    struct a2v
	    {
	    	float4 vertex:POSITION;
	    	float2 texcoord:TEXCOORD0;
	    	float4 color:COLOR;
	    	float4 normal:NORMAL;
	    };
	    
	    struct v2f
	    {
	    	float4 pos:SV_POSITION;
	    	float4 color:COLOR;
	    };
	    
	    v2f vert(a2v v)
	    {
	    	v2f o;
			o.pos = UnityObjectToClipPos(v.vertex);
	    
	    	//ObjSpaceViewDir返回点到视点的一个向量
	    	float3 viewDir = normalize(ObjSpaceViewDir(v.vertex));
	    
	    	float rim = 1 - saturate(dot(viewDir,v.normal));
	    
	    	//遮罩的颜色*rim的_RimPower次方  让颜色变化曲线更陡峭
	    	o.color = _RimColor*pow(rim,_RimPower);
	    	return o;
	    }
	    
	    
	    float4 frag(v2f i) :COLOR
	    {
			return i.color;
	    }
	    	ENDCG
	   }
	    
	    	//正常显示
	    pass
	    {
	    	ZWrite On
	    	ZTest less
	    
	        CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

			sampler2D _MainTex;
		    float4 _MainTex_ST;

		  struct a2v
		  {
		  	float4 vertex:POSITION;
		  	float2 texcoord:TEXCOORD0;
		  };
		  
		  struct v2f
		  {
		  	float4 pos:POSITION;
		  	float2 uv:TEXCOORD0;
		  };
		  
		  
		  v2f vert(a2v v)
		  {
		  	v2f o;
			o.pos = UnityObjectToClipPos(v.vertex);
		  	o.uv = v.texcoord;
		  	return o;
		  }
		  
		  float4 frag(v2f i) :COLOR
		  {
		  	float4 texCol = tex2D(_MainTex,i.uv);
		  	return texCol;
		  }
			ENDCG
	   }
		
	}
	FallBack"Diffuse"
}
