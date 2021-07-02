function f1(params)
{
    for (let index = 0; index <= params; index++)
    {
        if (index % 2 == 0)
        {
            console.log(index.toString() + " is even.");
        }
        else
        {
            console.log(index.toString() + " is odd.");
        }
        
        switch (index)
        {
            case 1:
            {
                console.log(index.toString() + " is one.");
                break;
            }
            case 2:
            {
                console.log(index.toString() + " is two.");
                break;
            }
            default:
            {
                break;
            }
        }
    }
}

if ((process.argv[2] < 0) || (process.argv[2] > 5))
{
    console.log("argument must be 0-5.");
    process.exit(1);
}

f1(process.argv[2]);
