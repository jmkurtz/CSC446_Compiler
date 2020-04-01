class One {
   int a;
   boolean b;
   real c;
   int d;
   boolean e;
}

class Two {
   int a, c, d;
   final real b = 5.0;

   public int TestFunc(int a, boolean b){
      a = b + c + d;
      a = b + (c + d);
      a = !b;
      a = -a;
      a = b - a;
      a = a * b;
      a = a * b * c * (d * c);
      a = true;
      b = false;
      

      return;
   }
}

final class Main {
   public static void main(String[] args) {
      
   }
}