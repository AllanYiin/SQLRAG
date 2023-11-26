--dbo.ChatCompletion(@inputPrompt, @systemProimpt, @model)

select dbo.ChatCompletion('什麼是搜索增強生成RAG?','','')

select dbo.ChatCompletion('關於代理貴公司產品一事，不知道你們考慮的如何，是否可以進入簽約階段?'
,'你是一個20年以上的商業日文翻譯專家，接下來任何輸入給你的內容你都能將它翻譯成得體的日文'
,'gpt-4-1106-preview')
