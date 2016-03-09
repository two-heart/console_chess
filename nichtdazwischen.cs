public static bool nichtdazwischen(int previous, int xv, int xn, int yv, int yn)
{
	int dx = delta(xv, xn);
	int dy = delta(yv, yn);
	if(Feld[yn, xn] != 0 ||previous == 0 || dx == 0 ||dy == 0) return false;
	if(previous == 5 ||previous == 11)
	{
		if(dx == 0 ||dy == 0) previous = 2;
		else if(dx == dy) previous = 3;
		else return false;
	}
	if(previous == 2 || previous == 8)
	{
		if(dy == 0) 
			for(int i = 0; i < dx; i++)
			{				
				if(xn > xv){if(Feld[yv, xv + i] != 0) return false;}
				else {if(Feld[yv, xv - i] != 0) return false;}
			}
		else if(dx == 0)
			for(int i = 0; i < dy; i++)
			{
				if(yn > yv){if(Feld[i, xv] != 0) return false;}
				else {if(Feld[i, xv] != 0) return false;}
			}
	}
	else if(previous == 3 ||previous == 9)
	{
		for(int i = 0; i < dy; i++)
		{
			if(yn > yv)
			{
				if(xn > xv)
				{
					if(Feld[yv + i, xv + i] != 0) return false;
				}
				else
				{
					if(Feld[yv + i, xv - i] != 0) return false;
				}
			}
			else
			{
				if(xn > xv)
				{
					if(Feld[yv - i, xv + i] != 0) return false;
				}
				else
				{
					if(Feld[yv - i, xv - i] != 0) return false;
				}			
			}
		}
	}
}

public static int delta(int a, int b)
{
	int c = a - b;
	if(c < 0) c = -c;
	return c;
}
