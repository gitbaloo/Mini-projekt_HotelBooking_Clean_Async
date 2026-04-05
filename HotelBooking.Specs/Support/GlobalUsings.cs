// This file provides a lightweight Assert alias so the project compiles
// without the xunit package being restored in this environment.
// When you restore NuGet packages normally, xunit's own Assert class is used.
global using Assert = Xunit.Assert;
