I uploaded an example unit test and claude generated unit
tests using the same arrange act assert framework I was using.
It also only used the same libraries and helpers I  was using.
The code claude generated contained an error. It was capturing a
variable from an outer scope. I manually refactored the fix. 