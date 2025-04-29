# Locks
A Lox interpreter I wrote in C# that I had started and completed in order to understand more thoroughly of how to make OOP languages.

One thing I added was having classes have an optional `tostr` method

```
class Car {
  init(year, location) {
    this.year = year;
    this.location = location;
  }
  drive(distance) {
    this.location = this.location + distance;
  }
  tostr() {
    return "Car " + this.year + ", " + this.location;
  }
}
class FlyingCar < Car {
  init(year, location, altitude) {
    super.init(year, location);
    this.altitude = altitude;
  }
  fly(distance) {
    this.altitude = this.altitude + distance;
  }
  tostr() {
    return "Flying " + super.tostr() + ", " + this.altitude;
  }
}

var car = Car(1984, 0);
car.drive(40);
print car; // Car 1984, 40

var flyingCar = FlyingCar(2015, 0, 0);
flyingCar.drive(100);
flyingCar.fly(50);
print flyingCar; // Flying Car 2015, 100, 50
```
