#include "pch.h"
#include <stdlib.h>
#include "Random.h"

using namespace std;
int Random::m_last_seed = 0x29;
int Random::Getone() {
	srand(Random::m_last_seed);
	Random::m_last_seed = rand() | rand() << 16;
	return Random::m_last_seed;
}
