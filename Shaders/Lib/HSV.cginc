﻿fixed3 rgb2hsv(fixed3 c)
{
	fixed4 K = fixed4(0.0, -1.0 / 3.0, 2.0 / 3.0, -1.0);
	fixed4 p = lerp(fixed4(c.bg, K.wz), fixed4(c.gb, K.xy), step(c.b, c.g));
	fixed4 q = lerp(fixed4(p.xyw, c.r), fixed4(c.r, p.yzx), step(p.x, c.r));

	fixed d = q.x - min(q.w, q.y);
	fixed e = 1.0e-10;
	return fixed3(abs(q.z + (q.w - q.y) / (6.0 * d + e)), d / (q.x + e), q.x);
}

fixed3 hsv2rgb(fixed3 c)
{
	fixed4 K = fixed4(1.0, 2.0 / 3.0, 1.0 / 3.0, 3.0);
	fixed3 p = abs(frac(c.xxx + K.xyz) * 6.0 - K.www);
	return c.z * lerp(K.xxx, clamp(p - K.xxx, 0.0, 1.0), c.y);
}
