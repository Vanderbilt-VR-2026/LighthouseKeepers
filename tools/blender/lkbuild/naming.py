import re
PATTERN=re.compile(r'^(?:SM_[A-Za-z0-9_]+|UCX_SM_[A-Za-z0-9_]+_\d{2,3}|SOCKET_[A-Za-z0-9_]+|GRIP_[A-Za-z0-9_]+|PIVOT_[A-Za-z0-9_]+|READOUT_[A-Za-z0-9_]+)$')
def valid(name):return bool(PATTERN.fullmatch(name))
