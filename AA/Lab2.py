import math

def bin(x):
    return "{0:b}".format(x)

def bin2(x):
    res = 0
    l = len(x)
    for i in range(l):
        res += 2 ** i * int(x[l - i - 1])
    return res


a = int(input(""))
b = int(input(""))
p = int(input(""))
m = int(input(""))
l = math.ceil(math.log((b - a) * (10 ** p), 2))
d = (b - a) / (2 ** l)

for i in range(m):
    type = input("")
    val = input("")
    if type == "TO":
        id = (float(val) - a) / d
        if len(str(bin(int(id)))) < l:
            print("0" * (l - len(str(bin(int(id))))) + bin(int(id)))
        else:
            print(bin(int(id)))
    else:
        if type == "FROM":
            id = bin2(val)
            res = a + id * d
            print(f"{res:.{l}f}")
