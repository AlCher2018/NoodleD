delete from Dish
delete from DishGarnish
delete from DishIngredient
delete from DishMark
delete from DishMarks
delete from DishRecommends
delete from DishUnit
delete from FieldType
delete from MenuFolder
delete from StringValue

dbcc checkident('Dish',reseed,0)
dbcc checkident('DishMark',reseed,0)
dbcc checkident('DishMarks',reseed,0)
dbcc checkident('DishRecommends',reseed,0)
dbcc checkident('DishUnit',reseed,0)
dbcc checkident('FieldType',reseed,0)
dbcc checkident('MenuFolder',reseed,0)
dbcc checkident('StringValue',reseed,0)

dbcc checkident('Order',reseed,0)
dbcc checkident('OrderDish',reseed,0)
dbcc checkident('Order',reseed,0)


