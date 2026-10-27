import math
from .primitives import mesh,arc,rod,polar

def flight(st):
 key=st['id'];r=st['walkLineRadius'];w=st['clearWidth'];ri,ro=r-w/2,r+w/2;y=st['floorY'];rise=st['rise'];start=st['departureAzimuth'];span=st['arcDegrees']
 # Visual treads are independent of smooth locomotion collision. The first tread is flush at departure;
 # nineteen rises are represented over eighteen between-nosing goings plus the end transition.
 for i in range(18):
  a=start-span*i/18;b=start-span*(i+1)/18;h=y+rise*(i+1)/19
  arc('SM_Stair_'+key+'_Tread_'+str(i),ri,ro,h-.08,h,b,a,'Metal',2,False)
 # Smooth closed ramp is the only walking collider; taper the start/end to the authored floors.
 verts=[];faces=[];n=72
 for i in range(n+1):
  a=start-span*i/n;h=y+rise*i/n
  for rad,yy in [(ri,h-.14),(ro,h-.14),(ri,h),(ro,h)]:verts.append(polar(a,rad,yy))
 for i in range(n):
  q=i*4;k=q+4;faces.extend([(q+2,k+2,k+3,q+3),(q,q+1,k+1,k),(q,q+2,k+2,k),(q+1,k+1,k+3,q+3)])
 faces += [(0,2,3,1),(n*4,n*4+1,n*4+3,n*4+2)]
 mesh('SM_Stair_'+key+'_Ramp',verts,faces,'Metal',True,True)
 for side,rad in [('Inner',ri-.0225),('Outer',ro+.0225)]:
  count=max(2,int(math.radians(span)*rad/.12)+1)
  for i in range(count+1):
   a=start-span*i/count;h=y+rise*i/count
   rod('SM_Rail_'+key+'_'+side+'_Post_'+str(i),polar(a,rad,h-.12),polar(a,rad,h+1.05),.018,collision=False)
   if i<count:
    b=start-span*(i+1)/count;hb=y+rise*(i+1)/count
    rod('SM_Rail_'+key+'_'+side+'_Top_'+str(i),polar(a,rad,h+1.05),polar(b,rad,hb+1.05),.0225,collision=False)
    rod('SM_Rail_'+key+'_'+side+'_Stringer_'+str(i),polar(a,rad,h-.05),polar(b,rad,hb-.05),.06,collision=False)
  # Continuous low-poly barrier behind visible rails prevents falling through baluster gaps.
  rv=[];rf=[]
  for i in range(n+1):
   a=start-span*i/n;h=y+rise*i/n
   for rr,yy in [(rad-.02,h),(rad+.02,h),(rad-.02,h+1.05),(rad+.02,h+1.05)]:rv.append(polar(a,rr,yy))
  for i in range(n):
   q=i*4;k=q+4;rf.extend([(q,q+2,k+2,k),(q+1,k+1,k+3,q+3),(q+2,q+3,k+3,k+2),(q,k,k+1,q+1)])
  rf += [(0,1,3,2),(n*4,n*4+2,n*4+3,n*4+1)]
  ob=mesh('SM_Safety_'+key+'_'+side,rv,rf,'Metal',True,True);ob['collision_only']=True
