brute
=====

Brute is my attempt to understand brute force password recovery. 
How to speed up password generation.
How to speed up applying those passwords to opening the file that is password protected.

It's been rather interesting.

On my modest system the passwords generated were around 20,000/second.
Once I applied it to open the Access database it dropped to around 8/second.
I introduced multithreading and the rate increased to around 200 or 250 passwords/second. 
Still very slow. I'd like to learn more about this step and see if I can improve on it. 


I'm building upon:

Brute Force
By Linoxxis de Flemming, 15 May 2009 

http://www.codekeep.net/snippets/d95707ab-7696-448c-8897-af97c42888b7.aspx

http://www.codeproject.com/Articles/36466/Brute-Force


