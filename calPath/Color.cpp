#include "Color.h"
Color::Color()
{
    setARGB(0, 0, 0, 0);
}
Color::Color(unsigned char red, unsigned char green, unsigned char blue)
{
    setARGB(0, red, green, blue);
}
Color::Color(unsigned char alpha, unsigned char red, unsigned char green, unsigned char blue)
{
    setARGB(alpha, red, green, blue);
}
void Color::setARGB(unsigned char alpha, unsigned char red, unsigned char green, unsigned char blue)
{
    m_alpha = alpha;
    m_red = red;
    m_green = green;
    m_blue = blue;
}