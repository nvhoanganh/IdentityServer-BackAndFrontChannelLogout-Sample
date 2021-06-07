# Demo of how backchannel logout works with DuendeIdentity Server

- run `IdentityServer`, `MvcBackChannelLogout` and `MvcBackChannelLogout2` projects at the same time by using `dotnet run` in the correct folder
- open `https://localhost:44300` and `https://localhost:44301`
- login from `https://localhost:44300` by click on `Secure` tab, use `alice` and `alice` as username and password
- then on `https://localhost:44301` , click on `Secure` tab again -> you will login straightaway
- then `https://localhost:44300` click on log out -> you see the logged out confirmation page on IDS
- on `https://localhost:44301`, reload the page -> you see tht you get kicked out as well

- NOTE: so you can see that if you logged in to 2 different apps that uses OIDC on the same browser (sharing the same IDS session), when you logged out from 1 app, the other app session will also end. this is done using backend channel (IDS is making a direct call to each of the client app). See more https://ashend.medium.com/openid-connect-backchannel-logout-144a3198d2a and `Clients.cs` file in the IDS


# Demo of how front-channel logout works with DuendeIdentity Server

- run `IdentityServer`, `MvcJarJwt` and `MvcJarJwt2` projects at the same time by using `dotnet run` in the correct folder
- open `https://localhost:44302` and `https://localhost:44303`
- login from `https://localhost:44302` by click on `Secure` tab, use `alice` and `alice` as username and password
- then on `https://localhost:44303` , click on `Secure` tab again -> you will login straightaway
- then `https://localhost:44302` click on log out -> you see the logged out confirmation page on IDS
- on `https://localhost:44303`, reload the page -> you see tht you get kicked out as well
- come back to the 1st tab (where you see "you have logged out" page from IDS), inspect element => you should see an Iframe with 2 links back to https://localhost:44302 and https://localhost:44303

- NOTE: similar to backend channel, we can achieve the same effect using the iframe (front channel logout url) and `Clients.cs` file in the IDS