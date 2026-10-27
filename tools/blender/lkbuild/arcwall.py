import math
from .primitives import mesh,polar

def wall(name,r,outer0,outer1,y0,y1,a0,a1,surface='Stone'):
 # Inner polygon is circumscribed: wall faces never intrude inside the specified clear radius.
 mid=(a0+a1)/2;ri=r/math.cos(math.radians((a1-a0)/2));vs=[]
 for y,ro in [(y0,outer0),(y1,outer1)]:
  vs += [polar(a0,ri,y),polar(a1,ri,y),polar(a1,ro,y),polar(a0,ro,y)]
 return mesh(name,vs,[(0,3,2,1),(4,5,6,7),(0,1,5,4),(3,7,6,2),(0,4,7,3),(1,2,6,5)],surface,True)
