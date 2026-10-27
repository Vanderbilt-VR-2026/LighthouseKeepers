def classification(name):
 return 'static' if any(x in name for x in ['Floor','Ramp','Safety','Landing','Gallery']) else 'convex'
