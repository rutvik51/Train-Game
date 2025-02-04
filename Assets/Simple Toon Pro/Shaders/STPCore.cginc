#ifndef STPCORE_INCLUDED
#define STPCORE_INCLUDED

#include "STPFunctions.cginc"

sampler2D _MainTex;
float4 _MainTex_ST;
float4 _Color;
float4 _DarkColor;
float _AmbientCol;
float _ColIntense;
float _ColBright;

bool _Segmented;
float _Steps;
float _StpSmooth;
float _Offset;

bool _Clipped;
float _MinLight;
float _MaxLight;
float _Lumin;

float _MaxAtten;
float _PostAtten;

float4 _ShnColor;
bool _ShnOverlap;
float _ShnIntense;
float _ShnRange;
float _ShnSmooth;

float4 _RimColor;
bool _RimLimit;
bool _RimOverlap;
float _RimIntense;
float _RimTolerc;
float _RimSmooth;

float4 _SpcColor;
float _SpcLimit;
float _SpcOverlap;
float _SpcIntence;
float _SpcTolerc;
float _SpcSmooth;

float Toon (float dot, fixed atten)
{
	float offset = clamp(_Offset, -1, 1);
	float delta = _MaxLight - _MinLight;

	//intense
	float ints_pls = dot + offset;
	float ints_max = 1.0 + offset;
	float intense = clamp01(ints_pls / ints_max);

	//lit
	float step = 1.0 / floor(_Steps);
	int lit_num = ceil(intense / step);
	float lit = lit_num * step;

	//smooth
	float reduce_v = _Offset - 1.0;
	float reduce_res = 1.0 - clamp01(reduce_v / 0.1);  //!v offset plus
	float reduce = lit_num == 1 ? reduce_res : 1;

	float smth_start = lit - step;
	float smth_end = smth_start + step * _StpSmooth;

	float smth_lrp = invLerp01(smth_end, smth_start, intense);
	float smth_stp = smoothstep(smth_end, smth_start, intense, 0.);

	float smooth_v = smoothlerp(smth_stp, smth_lrp, _StpSmooth);
	float smooth = clamp01(lit - smooth_v * reduce * step);

	//shadow
	float atten_inv = clamp(atten, 1.0 - _MaxAtten, 1);
	float dimLit = smooth * atten_inv;
	float dim_dlt = dimLit - _MinLight;

	//luminocity
	float lumLight = _MaxLight + _Lumin;
	float lum_dlt = lumLight - _MinLight;

	//clipped
	float litd_clmp = clamp01(dim_dlt);
	float clip_cf = litd_clmp / delta;

	float clip_uncl = _MinLight + clip_cf * lum_dlt;
	float clip_v = clamp(clip_uncl, _MinLight, lumLight);

	//relative limits
	float lerp_v = lum_dlt * dimLit;
	float relate_v = _MinLight + lerp_v;

	//result
	float result = _Clipped * clip_v;
	result += !_Clipped * relate_v;

	return result;
}

//post effects

void PostShine (inout float4 col, float dot, float atten)
{
	float pos = abs(dot - 1.0);
	float len = _ShnRange * 2;

	float smth_inv = 1.0 - _ShnSmooth;
	float smth_end = len * smth_inv;

	float shine = posz(len - pos);
	float smooth = smoothstep(len, smth_end, pos, 1.);
	float dim = 1.0 - _MaxAtten * rev(atten) * rev(_ShnOverlap);

	float blend = _ShnIntense * shine * smooth * dim;
	col = ColorBlend(col, _ShnColor, blend);
}

void PostRim (inout float4 col, float toon, float atten, float NdotL, float VdotN)
{
	float offset = clamp(_Offset, -1, 1);
	float tol_smth = _RimTolerc * _RimSmooth;

	float ins_ovr = invLerp01(1.1, 1.0, _Offset);
	float rev_ovr = rev(ins_ovr);

	float ldot_ilp = invLerp01(-offset, 1, NdotL);
	float ldot_lv = clamp01(ldot_ilp + rev_ovr + !_RimLimit);

	float rim_rw = rev01(VdotN);
	float rim_mult = rim_rw * ldot_lv;

	float rim_min = rev(_RimTolerc);
	float rim_max = rim_min + tol_smth;

	float rim = smoothstep(rim_min, rim_max, rim_mult);
	float dim = 1.0 - _MaxAtten * rev(atten) * rev(_RimOverlap);

	float blend = _RimIntense * rim * dim;
	col = ColorBlend(col, _RimColor, blend);
}

void PostSpecular (inout float4 col, float toon, float atten, float NdotL, float NdotH, float VdotN, float FdotV)
{
	float ldot_p = 0.1;
	float stren_c = _SpcTolerc / 0.08 * 0.1;
	float orient_c = stren_c;
	float mlight_p = 0.1;
	float lim_smth = clamp01(NdotL / 0.1);

	NdotH = clamp01(NdotH);
	VdotN = clamp01(VdotN);
	FdotV = clamp01(FdotV);

	float stren_v = rev(VdotN) * stren_c;
	float orien_v = lerp01(rev(orient_c), 1, FdotV);
	float dot_v = clamp01(NdotH - stren_v) * orien_v;

	float dim = 1.0 - _MaxAtten * rev(atten) * rev(_SpcOverlap);
	float light_plus = _MinLight + mlight_p;

	float lit_dim = smoothstep(_MinLight, light_plus, toon);
	float lit_min = toon <= light_plus ? lit_dim : 1;
	float lim_v = _SpcLimit ? lit_min : 1;

	float spc_str = rev(_SpcTolerc);
	float smth_end = spc_str + (_SpcTolerc * _SpcSmooth);

	float specular = pos(dot_v - spc_str) * pos(NdotL + ldot_p);
	float smooth_v = smoothstep(spc_str, smth_end, dot_v) * lim_smth;

	float blend = _SpcIntence * specular * dim * lim_v * smooth_v;
	col = ColorBlend(col, _SpcColor, blend);
}

void PostAttenuation (inout float4 col, float atten)
{
	float flat_v = clamp01((_PostAtten - 1.0) / 0.1);

	float atten_r = lerp(atten, 0, flat_v);
	float atten_m = rev(atten_r) * pos(rev(atten));

	float atten_v = _PostAtten * atten_m;
	col -= colmagnmax(col) * atten_v;
}

float4 PostEffects (float4 col, float toon, float atten, float NdotL, float NdotH, float VdotN, float FdotV)
{
	PostShine(col, NdotL, atten);
	PostRim(col, toon, atten, NdotL, VdotN);
	PostSpecular(col, toon, atten, NdotL, NdotH, VdotN, FdotV);
	PostAttenuation(col, atten);

	return col;
}

#endif
