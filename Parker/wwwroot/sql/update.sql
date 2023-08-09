INSERT INTO config ("Name","Key","Value","ParentId") SELECT '问答','问答','问答',9 WHERE NOT EXISTS (SELECT 1 FROM config WHERE ParentId=9 AND "Key"='问答');
INSERT INTO config ("Name","Key","Value","ParentId") SELECT '文案','文案','文案',9 WHERE NOT EXISTS (SELECT 1 FROM config WHERE ParentId=9 AND "Key"='文案');
INSERT INTO config ("Name","Key","Value","ParentId") SELECT '天气','天气','天气',9 WHERE NOT EXISTS (SELECT 1 FROM config WHERE ParentId=9 AND "Key"='天气');
INSERT INTO config ("Name","Key","Value","ParentId") SELECT '舔狗','舔狗','舔狗',9 WHERE NOT EXISTS (SELECT 1 FROM config WHERE ParentId=9 AND "Key"='舔狗');
INSERT INTO config ("Name","Key","Value","ParentId") SELECT '艾特作图','艾特作图','艾特作图',9 WHERE NOT EXISTS (SELECT 1 FROM config WHERE ParentId=9 AND "Key"='艾特作图');