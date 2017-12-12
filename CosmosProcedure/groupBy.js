function groubBy(categories) {

    let context = getContext();
    let collection = context.getCollection();
    let response = context.getResponse();
    let result = {}
    categories.forEach(category => {
        // カテゴリごとに、集計関数を実行する
        collection.queryDocuments(collection.getSelfLink(),
            `select VALUE count(c) from c where c.category = "${category}"`, {},
            (err, documents, responseOptions) => {
                result[category] = documents[0]

                // check all query results finished.
                if (Object.keys(result).length == categories.length) {
                    response.setBody(result)
                }
            });
    });

}