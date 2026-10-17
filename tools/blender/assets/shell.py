SPEC = {'family':'Shell','tri_budget_lod0':90000,'materials_max':1,'moving_parts':[],'readouts':[],'grips':[],'reach_envelope_m':{'min':.75,'max':1.45},'clearance_m':1.0,'lod_decision':'Shell openings retain full topology; exterior rocks have authored LODs.','interactive':False}
import math,bpy
from lkbuild.primitives import box,arc,rod,mesh,polar,empty
from lkbuild.arcwall import wall
from lkbuild.stairs import flight

def outer(layout,y):
 pts=layout['exteriorProfile']
 for a,b in zip(pts,pts[1:]):
  if a[0]<=y<=b[0]:return a[1]+(b[1]-a[1])*(y-a[0])/(b[0]-a[0])
 return pts[-1][1] if y>pts[-1][0] else pts[0][1]

def floor_mesh(name,r,y,cut=None):
 # One watertight slab: shared top/bottom fans, no coincident internal wedge faces.
 # Internal faces in the first greybox caused radial bake seams and invalid texels.
 angles=sorted(set(list(range(0,361,3))+([cut[0],cut[1]] if cut else [])))
 outline=[]
 for a,b in zip(angles,angles[1:]):
  radius=cut[2] if cut and cut[0] <= (a+b)/2 <= cut[1] else r/math.cos(math.radians(1.5))
  for angle in [a,b]:
   point=polar(angle,radius,y)
   if not outline or sum((v-w)**2 for v,w in zip(point,outline[-1]))>1e-12:outline.append(point)
 if sum((v-w)**2 for v,w in zip(outline[0],outline[-1]))<1e-12:outline.pop()
 n=len(outline);verts=[(0,y,0),(0,y-.35,0)]+outline+[(x,y-.35,z) for x,_,z in outline];faces=[]
 for i in range(n):
  j=(i+1)%n
  faces.extend([(0,2+i,2+j),(1,2+n+j,2+n+i),(2+i,2+n+i,2+n+j,2+j)])
 return mesh(name,verts,faces,'Timber',True,True)

def reveal(f,op,index,layout):
 a=op['azimuth'];r=f['interiorRadius'];y=f['floorY'];h=op['height'];sill=op['sill'];w=op['width'];ro=outer(layout,y+sill+h/2);key=f['id']+'_'+str(index)
 yaw=a;angle=math.radians(a)
 def pt(x,yy,rad):return (rad*math.sin(angle)+x*math.cos(angle),yy,rad*math.cos(angle)-x*math.sin(angle))
 for side in [-1,1]:
  inner=side*(w/2+.05);outside=side*w/2
  vs=[pt(inner,y+sill,r),pt(outside,y+sill,ro),pt(outside,y+sill+h,ro),pt(inner,y+sill+h,r)]
  mesh('SM_Reveal_'+key+('_L' if side<0 else '_R'),vs,[(0,1,2,3),(3,2,1,0)],'Trim',False)
  # Narrow side strip finishes the clear aperture without placing a collider across it.
  box('SM_Reveal_'+key+'_Frame_'+str(side),pt(side*(w/2+.025),y+sill+h/2,r+.04),(.05,h,.08),'Metal' if op['type']=='window' else 'Trim',yaw)
 for suffix,yy in [('Sill',y+sill),('Lintel',y+sill+h)]:
  if op['type']=='door' and suffix=='Sill':continue
  box('SM_Reveal_'+key+'_'+suffix,pt(0,yy,(r+ro)/2),(w+.12,.06,ro-r+.1),'Trim',yaw)
 if op['type']=='window':
  box('SM_Glass_'+key,pt(0,y+sill+h/2,ro-.025),(w,h,.015),'Glass',yaw)
  for i,x in enumerate([-.24,0,.24]):rod('SM_Bar_'+key+'_'+str(i),pt(x,y+sill,ro-.07),pt(x,y+sill+h,ro-.07),.015,'Metal',collision=False)


def room(layout,f):
 id=f['id'];r=f['interiorRadius'];y=f['floorY'];h=f['clearHeight']
 incoming=next((s for s in layout['stairs'] if s['to']==id),None)
 cut=(incoming['arrivalAzimuth'],incoming['departureAzimuth'],incoming['walkLineRadius']-incoming['clearWidth']/2+.025) if incoming else None
 floor_mesh('SM_Floor_'+id,r,y,cut)
 for l in layout['landings']:
  if l['floor']==id:arc('SM_Landing_'+l['id'],l['innerRadius'],l['outerRadius'],y-.06,y+.001,l['arcStartDeg'],l['arcEndDeg'],'Metal',collision=False)
 openings=[o for o in layout['openings'] if o['floor']==id]
 if id!='F4':
  bounds=set(range(0,361,4))
  for o in openings:
   o=dict(o);half=math.degrees(math.asin((o['width']/2+.055)/r));bounds.update([o['azimuth']-half,o['azimuth']+half])
  bounds=sorted(bounds)
  for i,(a,b) in enumerate(zip(bounds,bounds[1:])):
   mid=(a+b)/2;op=next((o for o in openings if abs(mid-o['azimuth'])<math.degrees(math.asin((o['width']/2+.055)/r))-.00001),None)
   spans=[(0,h)] if not op else [(0,op['sill']),(op['sill']+op['height'],h)]
   for part,(lo,hi) in enumerate(spans):
    if hi-lo>.001:wall('SM_Wall_'+id+'_'+str(i)+'_'+str(part),r,outer(layout,y+lo),outer(layout,y+hi),y+lo,y+hi,a,b,'Plaster')
  for i,o in enumerate(openings):reveal(f,o,i,layout)
  if id!='F3':
   for i in range(90):wall('SM_Wall_'+id+'_Spandrel_'+str(i),r,outer(layout,y+h),outer(layout,y+3.3),y+h,y+3.3,i*4,(i+1)*4,'Stone')
  arc('SM_Trim_'+id+'_Ceiling',r-.08,r+.015,y+h-.08,y+h,0,360,'Timber',collision=False)
 else:
  # Glazed lantern ring with a genuine opening at east, never a pane across the gallery door.
  door_half=math.degrees(math.asin(.6/r));cuts=sorted(set(range(0,361,45))|{90-door_half,90+door_half})
  for i,(a,b) in enumerate(zip(cuts,cuts[1:])):
   mid=(a+b)/2
   if abs(mid-90)<door_half-.001:continue
   arc('SM_Glass_Lantern_'+str(i),r+.035,r+.05,y+.18,y+h-.12,a,b,'Glass',3)
   rod('SM_Frame_Lantern_'+str(i),polar(a,r+.035,y),polar(a,r+.035,y+h),.035,'Metal')
  for i,a in enumerate([90-door_half,90+door_half]):rod('SM_Frame_Door_'+str(i),polar(a,r+.025,y),polar(a,r+.025,y+2.1),.025,'Metal')
  arc('SM_Frame_Lantern_Base_A',r,r+.15,y-.02,y+.18,0,90-door_half,'Metal')
  arc('SM_Frame_Lantern_Base_B',r,r+.15,y-.02,y+.18,90+door_half,360,'Metal')
  arc('SM_Frame_Lantern_Top',r-.035,r+.10,y+h-.12,y+h,0,360,'Metal')
  # Steel curb supports glazing and ensures the gallery joins flush at Y9.90.
  arc('SM_Floor_Lantern_Threshold',r,3.1,y-.35,y,0,360,'Metal')
 for st in layout['stairs']:
  if st['from']==id:flight(st)
 # Guard the inner boundary of the floor opening. Solid collision sits behind the open balusters.
 if incoming:
  a0,a1,ir=cut
  for i in range(int((a1-a0)/4)+1):
   a=a0+(a1-a0)*i/max(1,int((a1-a0)/4));rod('SM_Guard_'+id+'_Post_'+str(i),polar(a,ir-.025,y-.08),polar(a,ir-.025,y+1.05),.018,'Metal',collision=False)
  arc('SM_Guard_'+id+'_Rail',ir-.0475,ir-.0025,y+1.0275,y+1.0725,a0,a1,'Metal',collision=False)
  ob=arc('SM_Safety_'+id+'_Opening',ir-.045,ir-.005,y,y+1.05,a0,a1,'Metal');ob['collision_only']=True
 # Lamp housings belong to the shell; their baked Light components are added by the editor builder.
 for k,a in enumerate([30,150,270]):
  p=polar(a,r-.11,y+2.35);box('SM_Fixture_'+id+'_'+str(k),p,(.22,.32,.12),'Metal',a,False)
  box('SM_Lamp_'+id+'_'+str(k),polar(a,r-.18,y+2.35),(.13,.22,.035),'Lamp',a,False)
 for k,a in enumerate([90,270]):
  box('SM_EmergencyFixture_'+id+'_'+str(k),polar(a,r-.10,y+2.45),(.20,.16,.09),'Metal',a,False)
  box('SM_EmergencyLamp_'+id+'_'+str(k),polar(a,r-.16,y+2.45),(.14,.10,.025),'Emergency',a,False)
 if id=='F4':
  for k,a in enumerate([45,225]):
   rod('SM_Gallery_LampArm_'+str(k),polar(a,2.73,y+2.75),polar(a,3.2,y+2.75),.025,'Metal',collision=False)
   box('SM_Gallery_LampHousing_'+str(k),polar(a,3.2,y+2.70),(.24,.10,.24),'Metal',a,False)
   box('SM_Gallery_EmergencyLamp_'+str(k),polar(a,3.2,y+2.64),(.18,.035,.18),'Emergency',a,False)
  g=layout['gallery'];arc('SM_Gallery_Deck',g['innerRadius'],g['outerRadius'],y-.18,y,0,360,'Metal')
  arc('SM_Gallery_Toeboard',4.20,4.25,y,y+.12,0,360,'Metal')
  for i in range(120):
   a=i*3;b=(i+1)*3
   rod('SM_Gallery_Post_'+str(i),polar(a,4.22,y),polar(a,4.22,y+1.1),.025,'Metal',collision=False)
   for hgt in [.55,1.1]:rod('SM_Gallery_Rail_'+str(i)+'_'+str(int(hgt*100)),polar(a,4.22,y+hgt),polar(b,4.22,y+hgt),.025,'Metal',collision=False)
  ob=arc('SM_Safety_Gallery',4.20,4.25,y,y+1.1,0,360,'Metal');ob['collision_only']=True
  # Engraved grating ribs over an opaque metal deck; no transparent floor.
  for i in range(90):
   a=i*4;rod('SM_Gallery_Grating_'+str(i),polar(a,3.12,y+.003),polar(a,4.19,y+.003),.007,'Metal',collision=False)

def house(layout):
 z=-7.35;box('SM_Floor_House',(0,-.175,z),(7.9,.35,5.9),'Timber')
 for side in [-1,1]:box('SM_Wall_House_Side_'+str(side),(side*3.725,1.35,z),(.45,2.7,5.9),'Plaster')
 for label,zz in [('North',-4.625),('South',-10.075)]:
  for side in [-1,1]:box('SM_Wall_House_'+label+'_'+str(side),(side*2.275,1.35,zz),(3.35,2.7,.45),'Plaster')
  box('SM_Wall_House_'+label+'_Lintel',(0,2.425,zz),(1.2,.55,.45),'Plaster')
 box('SM_Floor_Throat',(0,-.175,-4.70),(1.4,.35,1.7),'Metal')
 for side in [-1,1]:box('SM_Wall_Throat_'+str(side),(side*.7,1.3,-4.7),(.3,2.6,1.7),'Stone')
 box('SM_Roof_Throat',(0,2.70,-4.7),(1.7,.2,1.7),'Stone')
 box('SM_Floor_Porch',(0,-.175,-10.85),(2.4,.35,2.1),'Stone')
 for side in [-1,1]:
  # Roof slopes north/south; triangular gables east/west.
  verts=[(-4.05,2.75,z+side*3.12),(4.05,2.75,z+side*3.12),(-4.05,4.15,z),(4.05,4.15,z),(-4.05,2.63,z+side*3.12),(4.05,2.63,z+side*3.12),(-4.05,4.03,z),(4.05,4.03,z)]
  mesh('SM_Roof_House_'+str(side),verts,[(0,1,3,2),(4,6,7,5),(0,4,5,1),(2,3,7,6),(0,2,6,4),(1,5,7,3)],'Slate')
  x=side*3.95;mesh('SM_Gable_House_'+str(side),[(x,2.7,z-2.95),(x,2.7,z+2.95),(x,4.1,z)],[(0,1,2),(2,1,0)],'Stone',False)
 box('SM_Chimney_Stack',(-3.2,4.05,z),(.7,2.0,.75),'Stone');box('SM_Chimney_Cap',(-3.2,5.1,z),(.85,.18,.9),'Trim')
 for name,zz,surface in [('Green',-9.85,'Door'),('Steel',-3.85,'Metal')]:
  box('SM_Door_'+name,(-.57,1.025,zz+.525),(1.05,2.05,.06),surface,90)
  empty('PIVOT_'+name+'DoorHinge',(-.57,0,zz))
  rod('SM_Handle_'+name,(-.50,.95,zz+.9),(-.50,1.15,zz+.9),.018,'Brass',collision=False)
 for a in [-2.4,2.4]:
  box('SM_Fixture_House_'+str(int(a*10)),(a,2.35,-4.9),(.22,.30,.12),'Metal',collision=False)
  box('SM_Lamp_House_'+str(int(a*10)),(a,2.35,-5.),(.14,.22,.04),'Lamp',collision=False)
 rod('SM_Fixture_House_PendantStem',(0,2.7,z),(0,2.42,z),.018,'Metal',collision=False)
 box('SM_Fixture_House_PendantShade',(0,2.43,z),(.32,.10,.32),'Metal',collision=False)
 box('SM_Lamp_House_Pendant',(0,2.37,z),(.22,.04,.22),'Lamp',collision=False)
 # The ceiling is above the whole walking route, independent of roof geometry.
 box('SM_Ceiling_House',(0,2.78,z),(7,.16,5),'Timber')

def exterior(layout):
 # Distinct 0.60 m structural band; retain clear F3 interior through Y9.55.
 for i,(a,b) in enumerate(zip(layout['exteriorProfile'],layout['exteriorProfile'][1:])):
  if a[0]<9.3 or b[0]>9.9:continue
  n=96;verts=[];faces=[]
  for q in range(n+1):
   ang=q*360/n;verts.extend([polar(ang,a[1],a[0]),polar(ang,b[1],b[0])])
  for q in range(n):faces.append((2*q,2*q+1,2*q+3,2*q+2))
  mesh('SM_Cornice_Profile_'+str(i),verts,faces,'Stone',False)
 for i in range(24):
  a=i*15;p0=polar(a,3.88,9.30);p1=polar(a,4.15,9.73)
  rod('SM_Cornice_Corbel_'+str(i),p0,p1,.095,'Stone',sides=6,collision=False)
 arc('SM_Cornice_Drip',4.12,4.22,9.74,9.79,0,360,'Trim',collision=False)
 # Dome and finial, faceted brass seam strips provide a readable silhouette.
 n=64;bands=12;verts=[];faces=[]
 for k in range(bands+1):
  t=k/bands*math.pi/2;r=3.13*math.cos(t);yy=13.3+1.3*math.sin(t)
  for q in range(n):verts.append(polar(q*360/n,max(.005,r),yy))
 for k in range(bands):
  for q in range(n):faces.append((k*n+q,k*n+(q+1)%n,(k+1)*n+(q+1)%n,(k+1)*n+q))
 mesh('SM_Roof_Lantern_Dome',verts,faces,'Slate',False)
 rod('SM_Roof_Finial',(0,14.55,0),(0,15.1,0),.07,'Brass',collision=False)
 for i in range(12):
  a=i*30
  for k in range(11):
   t=k/12*math.pi/2;t2=(k+1)/12*math.pi/2
   rod('SM_Roof_Seam_'+str(i)+'_'+str(k),polar(a,3.14*math.cos(t),13.3+1.3*math.sin(t)),polar(a,3.14*math.cos(t2),13.3+1.3*math.sin(t2)),.018,'Metal',collision=False)
 for lod,n in [(0,64),(1,32),(2,16)]:
  verts=[];faces=[]
  for ring,y in enumerate([-.12,-1.2,-3.8]):
   for i in range(n):
    a=i*2*math.pi/n;noise=1+.08*math.sin(a*7)+.04*math.cos(a*13);rad=[1,.92,.7][ring];verts.append((7.4*noise*math.sin(a)*rad,y,-3+12.8*noise*math.cos(a)*rad))
  verts.append((0,-.15,-3));centre=len(verts)-1
  for i in range(n):
   q=(i+1)%n;faces.append((centre,q,i))
   for k in range(2):faces.append((k*n+i,k*n+q,(k+1)*n+q,(k+1)*n+i))
  mesh('SM_Rock_Island_LOD'+str(lod),verts,faces,'Rock',True,True)
 for i in range(16):
  a=i*360/16;r=6.5+.7*math.sin(i*3);p=polar(a,r,-.65);box('SM_Rock_Outcrop_'+str(i),(p[0],-.65,p[2]-2.3),(1.2+.3*math.sin(i),1.4,1.6),'Rock',i*27)

def reference():
 box('SM_ScaleReference_Cube',(0,.5,0),(1,1,1),'Trim');box('SM_ScaleReference_Height',(1.3,.85,0),(.12,1.7,.12),'Metal')
