Domain module

1. An Aggregate root may contain multiple aggregates or entities
2. Recommended to use entities if your root aggregate contains other nested objects.

For example: If your User Aggregate Root contains a list of user Roles which are managed by Role Aggregate Root, you should setup your User Aggregate Root with Role Entity instead of Role Aggregate Root to eliminate unused fields which are provided in the the Aggregate class base. 
