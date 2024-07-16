List<Tuple<string, string>> GetOptions 是項目的類別標籤與其name，name用途給GetItem的name帶入查詢用的
List<Tuple<string, int>> GetItem 是取得項目的名稱還有價格，台幣沒在用小數點不要說為啥不用Dynamics
然後也過濾掉價格為1元的廣告項目，其他應用方式就自己看要怎麼改，反正總體就是字串跟整數而已
