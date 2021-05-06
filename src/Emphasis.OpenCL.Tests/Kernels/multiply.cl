﻿void kernel multiply(
	global int* a, 
	global int* b,
    int c) 
{
	const int x = get_global_id(0);
    b[x] = a[x] * c;
}