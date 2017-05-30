# NoCarJack

### Description

Yet pretty simple, but it should be efficient against the annoying NPC car-jacking and car stealing on roleplay based severs. This will force your players to find other means to reach their destination. The ability to steal cars is based on a "luck" factor.
No more random grand theft auto.

### Features

* Works on all vehicles (cars,bikes,trucks,helicopters,planes,boats,...)
* Prevent parked vehicle stealing
* Prevent any kind of grand theft auto (car-jacking, passenger car-jack, etc)
* Vehicle stealing/car-jacking is only allowed only by a predefined threshold
* Compatible with any vehicle control script
* Ability to skip a vehicle check with an event
* Low CPU usage

### Installation

1) Download <a class="attachment" href="/uploads/default/original/2X/5/5480aba43bd4e654d2d6e7e8c76ee4d39bd11417.zip">nocarjack.zip</a> (4.7 KB) (or from the release page) and extract the content in your `resources` directory
2) Add `- nocarjack` in your _AutoStartResources_ ( `citmp-server.yml`)
3) Restart server

### Usage
:warning: **I highly recommend you to try it on a dev server before trying to apply it on your existing server.**

A vehicle can only be stolen if the current player's random luck is greater than 90%.
The player's last driven vehicle is always saved, so you don't have to trigger events all the time.

To skip a vehicle check for a player, you must trigger an event called ```nocarjack:skipThisFrame``` with the specified player ID. This will make him able to enter any car for x miliseconds, so make sure to check server-side if your player is near the car you want him to enter!

**Example :** 

- A random player (ID : 45) can not enter police cars or steal cars without having a luck greater than 90%
- A police player (ID : 25) should be able to enter police cars at any time
```
if CheckIfNearCar(25, policecar) == true
   TriggerClientEvent('nocarjack:skipThisFrame', 25, 3500) #This will allow ID 25 to enter any car for 3.5seconds
end
```

**Another example :**

- A legit player (ID : 2) bought a new car in a vehicle shop, he should be able to enter it for the first time
```
if PlayerBoughtThisCar(2, "alpha", 25000) == true #pseudo code
   TriggerClientEvent('nocarjack:skipThisFrame', 2, 10000) #This will allow ID 2 to enter his car for 10 seconds
end
```

And voila, it's up to you to decide who has the right to steal cars or not.
