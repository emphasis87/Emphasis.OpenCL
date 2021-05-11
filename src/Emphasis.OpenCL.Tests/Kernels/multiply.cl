#ifndef TDepth
#define TDepth int
#endif

void kernel multiply(
	global TDepth* a, 
	global TDepth* b,
    int c)
{
	const int x = get_global_id(0);
    b[x] = a[x] * c;
}