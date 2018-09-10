#ifndef COLOR_H_
#define COLOR_H_

class Color
{
  public:
    Color(unsigned char red, unsigned char green, unsigned char blue);
    Color(unsigned char alpha, unsigned char red, unsigned char green, unsigned char blue);
    Color();
    void setRed(unsigned r){m_red=r;}
    unsigned getRed(){return m_red;}

    void setGreen(unsigned r){m_green=r;}
    unsigned getGreen(){return m_green;}

    void setBlue(unsigned r){m_blue=r;}
    unsigned getBlue(){return m_blue;}

    void setAlpha(unsigned r){m_alpha=r;}
    unsigned getAlpha(){return m_alpha;}

  private:
    unsigned m_red;
    unsigned m_green;
    unsigned m_blue;
    unsigned m_alpha;
    void setARGB(unsigned char alpha, unsigned char red, unsigned char green, unsigned char blue);
};

#endif