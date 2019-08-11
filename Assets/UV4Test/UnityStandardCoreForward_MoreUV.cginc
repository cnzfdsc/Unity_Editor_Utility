// Unity built-in shader source. Copyright (c) 2016 Unity Technologies. MIT license (see license.txt)

#ifndef UNITY_STANDARD_CORE_FORWARD_INCLUDED
#define UNITY_STANDARD_CORE_FORWARD_INCLUDED


#include "UnityStandardConfig.cginc"


#include "UnityStandardCore_MoreUV.cginc"
VertexOutputForwardBase_MoreUV vertBase (VertexInput_MoreUV v) { return vertForwardBase_MoreUV(v); }
VertexOutputForwardAdd_MoreUV vertAdd (VertexInput_MoreUV v) { return vertForwardAdd_MoreUV(v); }
half4 fragBase (VertexOutputForwardBase_MoreUV i) : SV_Target { return fragForwardBaseInternal_MoreUV(i); }
half4 fragAdd (VertexOutputForwardAdd_MoreUV i) : SV_Target { return fragForwardAddInternal_MoreUV(i); }


#endif // UNITY_STANDARD_CORE_FORWARD_INCLUDED
