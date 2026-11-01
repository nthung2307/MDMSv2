using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Application.DataTransferObjects.Templates;
using Application.Repositories.ReadOnly;
using Application.Services;
using Domain.Entities;
using Microsoft.Extensions.Logging;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Infrastructure.Services.Templates;

public class StatementPdfGenerator : IStatementPdfGenerator
{
    private const string DefaultLogoBase64 = "iVBORw0KGgoAAAANSUhEUgAAASoAAABsCAYAAADdTP/qAAAMQGlDQ1BJQ0MgUHJvZmlsZQAASImVVwdYU8kWnluSkEBCCSAgJfQmCEgJICWEFkB6EWyEJEAoMQaCiB1dVHDtYgEbuiqi2AGxI3YWwd4XRRSUdbFgV96kgK77yvfm++bOf/85858zZ+aWAUD9FFcszkE1AMgV5UtiQwIYY5NTGKRuQAY0oAusAJXLyxOzoqMjACyD7d/Lu5sAkbXXHGRa/+z/r0WTL8jjAYBEQ5zGz+PlQnwIALySJ5bkA0CU8eZT88UyDCvQlsAAIV4owxkKXCnDaQq8T24TH8uGuBkAFTUuV5IBAK0N8owCXgbUoPVB7CTiC0UAqDMg9s3NncyHOBViG2gjhlimz0z7QSfjb5ppQ5pcbsYQVsxFXlQChXniHO60/zMd/7vk5kgHfVjBqpYpCY2VzRnm7Xb25HAZVoO4V5QWGQWxFsQfhHy5PcQoJVMamqCwRw15eWyYM7jOAHXicwPDITaEOFiUExmh5NPShcEciOEOQQuF+Zx4iPUgXijIC4pT2myWTI5V+kLr0iVslpK/wJXI/cp8PZRmJ7CU+q8zBRylPkYryoxPgpgCsUWBMDESYhrEjnnZceFKm9FFmezIQRuJNFYWvwXEsQJRSIBCHytIlwTHKu1Lc/MG54ttzhRyIpX4QH5mfKgiP1gzjyuPH84FaxOIWAmDOoK8sRGDc+ELAoMUc8e6BaKEOKXOB3F+QKxiLE4R50Qr7XEzQU6IjDeD2DWvIE45Fk/MhxtSoY+ni/Oj4xVx4kVZ3LBoRTz4MhAB2CAQMIAU1jQwGWQBYWtvfS+8U/QEAy6QgAwgAA5KZnBEkrxHBK9xoAj8CZEA5A2NC5D3CkAB5L8OsYqrA0iX9xbIR2SDpxDngnCQA++l8lGiIW+J4AlkhP/wzoWVB+PNgVXW/+/5QfY7w4JMhJKRDnpkqA9aEoOIgcRQYjDRFjfAfXFvPAJe/WF1wZm45+A8vtsTnhLaCY8JNwgdhDuThMWSn6IcAzqgfrAyF2k/5gK3gppueADuA9WhMq6LGwAH3BX6YeF+0LMbZNnKuGVZYfyk/bcZ/LAaSjuyExklDyP7k21+Hkmzo7kNqchy/WN+FLGmDeWbPdTzs3/2D9nnwzb8Z0tsIXYQO4+dxi5ix7B6wMBOYg1YC3Zchod21xP57hr0FiuPJxvqCP/hb3BlZZnMc6px6nH6oujLFxTK3tGAPVk8TSLMyMxnsOAXQcDgiHiOIxguTi6uAMi+L4rX15sY+XcD0W35zs37AwCfkwMDA0e/c2EnAdjvAR//I985Gyb8dKgCcOEITyopUHC47EKAbwl1+KTpA2NgDmzgfFyAO/AG/iAIhIEoEA+SwUQYfSbc5xIwFcwAc0EJKAPLwGqwHmwCW8FOsAccAPXgGDgNzoHLoA3cAPfg7ukCL0AfeAc+IwhCQqgIHdFHTBBLxB5xQZiILxKERCCxSDKSimQgIkSKzEDmIWXICmQ9sgWpRvYjR5DTyEWkHbmDPEJ6kNfIJxRD1VBt1Ai1QkeiTJSFhqPx6AQ0A52CFqHz0SXoWrQK3Y3WoafRy+gNtAN9gfZjAFPFdDFTzAFjYmwsCkvB0jEJNgsrxcqxKqwWa4TrfA3rwHqxjzgRp+MM3AHu4FA8AefhU/BZ+GJ8Pb4Tr8Ob8Wv4I7wP/0agEgwJ9gQvAocwlpBBmEooIZQTthMOE87CZ6mL8I5IJOoSrYke8FlMJmYRpxMXEzcQ9xJPEduJncR+EomkT7In+ZCiSFxSPqmEtI60m3SSdJXURfqgoqpiouKiEqySoiJSKVYpV9mlckLlqsozlc9kDbIl2YscReaTp5GXkreRG8lXyF3kzxRNijXFhxJPyaLMpayl1FLOUu5T3qiqqpqpeqrGqApV56iuVd2nekH1kepHNS01OzW22ng1qdoStR1qp9TuqL2hUqlWVH9qCjWfuoRaTT1DfUj9QKPTHGkcGp82m1ZBq6Ndpb1UJ6tbqrPUJ6oXqZerH1S/ot6rQdaw0mBrcDVmaVRoHNG4pdGvSdd01ozSzNVcrLlL86JmtxZJy0orSIuvNV9rq9YZrU46Rjens+k8+jz6NvpZepc2Udtam6OdpV2mvUe7VbtPR0vHVSdRp1CnQue4Tocupmuly9HN0V2qe0D3pu6nYUbDWMMEwxYNqx12ddh7veF6/noCvVK9vXo39D7pM/SD9LP1l+vX6z8wwA3sDGIMphpsNDhr0Dtce7j3cN7w0uEHht81RA3tDGMNpxtuNWwx7DcyNgoxEhutMzpj1Gusa+xvnGW8yviEcY8J3cTXRGiyyuSkyXOGDoPFyGGsZTQz+kwNTUNNpaZbTFtNP5tZmyWYFZvtNXtgTjFnmqebrzJvMu+zMLEYYzHDosbiriXZkmmZabnG8rzleytrqySrBVb1Vt3WedYc6yLrGuv7NlQbP5spNlU2122JtkzbbNsNtm12qJ2bXaZdhd0Ve9Te3V5ov8G+fQRhhOcI0YiqEbcc1BxYDgUONQ6PHHUdIxyLHesdX460GJkycvnI8yO/Obk55Thtc7rnrOUc5lzs3Oj82sXOhedS4XJ9FHVU8KjZoxpGvXK1dxW4bnS97UZ3G+O2wK3J7au7h7vEvda9x8PCI9Wj0uMWU5sZzVzMvOBJ8AzwnO15zPOjl7tXvtcBr7+8HbyzvXd5d4+2Hi0YvW10p4+ZD9dni0+HL8M31Xezb4efqR/Xr8rvsb+5P99/u/8zli0ri7Wb9TLAKUAScDjgPduLPZN9KhALDAksDWwN0gpKCFof9DDYLDgjuCa4L8QtZHrIqVBCaHjo8tBbHCMOj1PN6QvzCJsZ1hyuFh4Xvj78cYRdhCSicQw6JmzMyjH3Iy0jRZH1USCKE7Uy6kG0dfSU6KMxxJjomIqYp7HOsTNiz8fR4ybF7Yp7Fx8QvzT+XoJNgjShKVE9cXxideL7pMCkFUkdY0eOnTn2crJBsjC5IYWUkpiyPaV/XNC41eO6xruNLxl/c4L1hMIJFycaTMyZeHyS+iTupIOphNSk1F2pX7hR3CpufxonrTKtj8fmreG94PvzV/F7BD6CFYJn6T7pK9K7M3wyVmb0ZPpllmf2CtnC9cJXWaFZm7LeZ0dl78geyEnK2Zurkpuae0SkJcoWNU82nlw4uV1sLy4Rd0zxmrJ6Sp8kXLI9D8mbkNeQrw1/5FukNtJfpI8KfAsqCj5MTZx6sFCzUFTYMs1u2qJpz4qCi36bjk/nTW+aYTpj7oxHM1kzt8xCZqXNapptPnv+7K45IXN2zqXMzZ77e7FT8Yrit/OS5jXON5o/Z37nLyG/1JTQSiQltxZ4L9i0EF8oXNi6aNSidYu+lfJLL5U5lZWXfVnMW3zpV+df1/46sCR9SetS96UblxGXiZbdXO63fOcKzRVFKzpXjllZt4qxqnTV29WTVl8sdy3ftIayRrqmY23E2oZ1FuuWrfuyPnP9jYqAir2VhpWLKt9v4G+4utF/Y+0mo01lmz5tFm6+vSVkS12VVVX5VuLWgq1PtyVuO/8b87fq7Qbby7Z/3SHa0bEzdmdztUd19S7DXUtr0BppTc/u8bvb9gTuaah1qN2yV3dv2T6wT7rv+f7U/TcPhB9oOsg8WHvI8lDlYfrh0jqkblpdX31mfUdDckP7kbAjTY3ejYePOh7dccz0WMVxneNLT1BOzD8xcLLoZP8p8ane0xmnO5smNd07M/bM9eaY5taz4WcvnAs+d+Y86/zJCz4Xjl30unjkEvNS/WX3y3Utbi2Hf3f7/XCre2vdFY8rDW2ebY3to9tPXPW7evpa4LVz1znXL9+IvNF+M+Hm7Vvjb3Xc5t/uvpNz59Xdgruf7825T7hf+kDjQflDw4dVf9j+sbfDveP4o8BHLY/jHt/r5HW+eJL35EvX/KfUp+XPTJ5Vd7t0H+sJ7ml7Pu551wvxi8+9JX9q/ln50ublob/8/2rpG9vX9UryauD14jf6b3a8dX3b1B/d//Bd7rvP70s/6H/Y+ZH58fynpE/PPk/9Qvqy9qvt18Zv4d/uD+QODIi5Eq78VwCDFU1PB+D1DgCoyQDQ4fmMMk5x/pMXRHFmlSPwn7DijCgv7gDUwv/3mF74d3MLgH3b4PEL6quPByCaCkC8J0BHjRqqg2c1+blSVojwHLA58mtabhr4N0Vx5vwh7p9bIFN1BT+3/wJofHxqVHCdCQAAAIplWElmTU0AKgAAAAgABAEaAAUAAAABAAAAPgEbAAUAAAABAAAARgEoAAMAAAABAAIAAIdpAAQAAAABAAAATgAAAAAAAACQAAAAAQAAAJAAAAABAAOShgAHAAAAEgAAAHigAgAEAAAAAQAAASqgAwAEAAAAAQAAAGwAAAAAQVNDSUkAAABTY3JlZW5zaG90Xij7eAAAAAlwSFlzAAAWJQAAFiUBSVIk8AAAAdZpVFh0WE1MOmNvbS5hZG9iZS54bXAAAAAAADx4OnhtcG1ldGEgeG1sbnM6eD0iYWRvYmU6bnM6bWV0YS8iIHg6eG1wdGs9IlhNUCBDb3JlIDYuMC4wIj4KICAgPHJkZjpSREYgeG1sbnM6cmRmPSJodHRwOi8vd3d3LnczLm9yZy8xOTk5LzAyLzIyLXJkZi1zeW50YXgtbnMjIj4KICAgICAgPHJkZjpEZXNjcmlwdGlvbiByZGY6YWJvdXQ9IiIKICAgICAgICAgICAgeG1sbnM6ZXhpZj0iaHR0cDovL25zLmFkb2JlLmNvbS9leGlmLzEuMC8iPgogICAgICAgICA8ZXhpZjpQaXhlbFlEaW1lbnNpb24+MTA4PC9leGlmOlBpeGVsWURpbWVuc2lvbj4KICAgICAgICAgPGV4aWY6UGl4ZWxYRGltZW5zaW9uPjI5ODwvZXhpZjpQaXhlbFhEaW1lbnNpb24+CiAgICAgICAgIDxleGlmOlVzZXJDb21tZW50PlNjcmVlbnNob3Q8L2V4aWY6VXNlckNvbW1lbnQ+CiAgICAgIDwvcmRmOkRlc2NyaXB0aW9uPgogICA8L3JkZjpSREY+CjwveDp4bXBtZXRhPgpSn4hWAAAAHGlET1QAAAACAAAAAAAAADYAAAAoAAAANgAAADYAABnRvFUUvgAAGZ1JREFUeAHsXXl4VFWWf7VkIRAI2QMhhAQIIKu0oAPt5/L1qKM22tjQLdg2wkhLs3VrO27T6nTrDDRKA42gtMuION+M6KjQrqjdaIMyKhjZEpaEsCUkJCYhIUklVZOc3zlVvFdVea+KCoRw80dO3f2+c+8793fPOfc+287iao/W+lff6G4jmsNuI9pZ/nk81D0tKSGGupSd0k3XNUlvdiNfdZOL0hNioog67XZdfhUIjQMV1aepwJGT4Ksb00Sz2cDv000tlB4TDT7bPPr5kxDvpPSc1Fg0bHOARvi/m8ff1dxMNcu8sNn0/Ylwsxq4oGlRDjyXw6Gfb9KPFmZcczP41dH9Cvc5pb+xMTJOQsOtMTLlbEpQRYaRXbUWJajaH1klqNrnT6RSbQ+8tJN4vXFzDdUpAERWTG9DvII6HLxCcVjSOwqIeXiFvnxEPDX11MxhRGOi9JLe1YKVam8FnqOoHivrgF5xlH9EYnfpqqIBOFBUdopii8saiJZW1hP9aEcF0fwDQFaCqCiy9Z+rGfPB6QTUsjOCEYSTngxEdf33kqhIXt+eRFMYIQ/L6kHhmCjko0AY/76raaRSr/ythGhxGfobHYX+ybQNo+qARTwa6o2JAoKaND6N8o3MwXNKITczbNPXxyjqg2+qiMr0dUrHeOcg5c4XdXM/kntiB5PSC9TOgiE+DuHePbBjGdkf71XP7ojvqH4rQdVRnL3A6lWCKrQBU4LqHAuqv+8+SYhqyfqDNFJ1DUAmdkZMgqyaXAC5peVId2ssQWUlsEXzSCPd4wai0TRWaki+0OaDprFOI6En6l00azDVcOWIlIA1uVgH8D8HgQS2ltZSvssyehG9NBE6rhFJgrAYIQasretGFh4F8vy8ACv8xs9P0MMWFAGJNDVhvG12TEiPaGMQbZkxoovxuDF+TicqSOwNBHXNpb2prgnDgESuHZVMYZtNr+sxa7C8Gkhw8foCyvre1mqizS0Yb6+qysPz08PzUp6LEZJZO7501knZmyjq8RlZRCd/v78vS+svQVTrPwXSe/r1I5Rec0reF35Ob7/QP10l5zHg5ZuXP+hfN1YVTxgFhDw+D+M4JgfhYVl43yLVdZsSVEpQtU0mJahCnQdKULXNm3MmqDwemEtOVNW1tavVNbDVhEKa5mQrhiCt7UXIV10PySq6iKIyxG/JxwpdVcUDzyLZ0wIdguZdwbgBU4J6bHbWdUyE5F708yFUMjqIbqOJrT//uQ/I6v2D6FdcLOoZlwHdyJBesEZNTIMOLNaJdNNuXWAZZGXftL2Mer76Hazse/Zj3DSbPDf47eEVXmMkFPq4CYNEAAA52LxWMYTdzbAm9k4Ecps0MYUKCrIal5coFbVLXc1AaqWVQNBfHcRz7TkM+t4X5VS+tBTzUJBe+M8FQRUTi+d77OdAUrdO6Kfrp1jRauuAvPIPAcnmH0K/Pvv2JOXfsRs6QrcgF6MyUFfrOQj4KZ35PZT+eREvj68H45iXC53wpCswjpPGp1NnE3uy1TfMrtuUoFKCiuaOElQhvkJKUIFh50xQhas80o+rywWJuq0QK8aXB76jDK9vhu6jHAtaK55CPs2rw9LXEzyEFbh3Ivb2T84cSFmvHpkavEhriiCrlbuOU74tpUCM9iis4LYmrMDDUoGw+sdj8z0hCQJsoNdayAPSbmvnIhHI4duTWIGTYsGPPkGsLq4W5F/9DnSQr3xYSp2sroaORp7Kp1NE/nPxJNSGHS+8tz03etS/H8ZnxnWZlHTbxL5EBeF781v8sWk7nnvF24eoRIEgSUEIISN99Ds2DvSxO4GobvkHPaIy697hE3hflm9Av/6yGTsAH6A6x+Nh1mG/dIyXjcdRemtn3fR1E6FznHczdHg56dBh+VVjEtGKqJSgauORElRi/JCpZjJzIpWsBBVxUgmq9idUxASVfzOY8J/vhW5ozXsllOWLfLHGcAmvDsS/Bl2MOHjxivuTG4CkHpkKK6CTPYN1Zc4I1DYCyS3fjZV1exl0BnbWmYiuRGO5ncArZb9eQFhJrNv6x1SEsxNgNZR2gffOaDDCP0/UwRr3vyXgn3g6T83FiiXIytcs+L98YzFFrXnzKNHmZiAATwusZOHraHwtReYXVmZNxpmRTjcAW23GjUBWd1/fn5qLjTYgMYudeO3Tw5TzyXVFRE+fBp98ujiLFWlo/2wRlbR24BiQ1fxVuynqQInsPJhKxs5Ovbot0XkCuV8+EjuWX9+aQ08wMge6ZquPowSVElRW50oH51OCqo3BSlAFnmYdKKj0DVbUwNP5+fdhbXr5HeiMWhghmeusZCKDJidBN7PsnqHU0NhB1iR0eR2QxNJdZVSusFK/YnlYOeCIQv0aWzc8zUBgogpK4h9xTmCpzFjQ8UlAXOlxKB8dDV2Lg5GCcadtY+QgfkqNrOvbXQV+bT0JK9WxU2i/rBb9vYmf905GVMJtscL+16dAUIteAXJwudA/n/VVSnRWinG2OcBHhwMr84wbYUVa8MMB1HFniFbaerZq/+aFvVT+oy8qmQGwYmt8ZtCcK5FFVILkn/vgEDW99NViojIvrPfLvOfnNIds7XnnNH50AjX/29sHEc3NgLXdrE9KUClBZTZHzlO6ElRtjFeCCtPvnAkqme2n6oEMHl4LD+IPt8CPRPx8JF9QysjEZgNSmXwtJPSjPx1CRaKc1nQXJxhZLdkJq2RRNfuFsV+Pp4WVy+wHZuN2HdFQmojndKt7B7XrbgJSi+VbBMS/S07TS3n/52IdCZNmPrN4ugF8atbwnHYnkEWcDe08NCaDqhrEujKpdztbW+9ZDl1HdTVeeE8LdFyS7+wpdzhoRWg3aLJpAtfPnvF2B/hw+/XwYH/oNlh95QyaaXWGDMs3FFHMqvUlRD1yguJ8ISruX34RdLpzV2L8TvBJEPMdh+EBO0sQQL61N/ghbnS3XQs/qwen5FFPY6NFpxW440pQKUEVeGaYxipB1caiSCnThd1KUAkn9PScCyppftch6AbuW1NIUUWHgSB81pf2XwQbr7CJvSGpV82/hOoZOQAIS9oxo8froQNakg9rYAkjqxYX98d7Jkxqkn4xYmAdliAmu5MREFshJV50XVKLH2Vro5yJEyukmz3sHdFAVKNTUf9DI/ugCm6/iT2zV2zYT/Fr3jiKdFnRfI45fk23H8HPyboGm8YrH58U8CvL/PJ4xN1BdD9AnpatjNyendv5wRW9qKnf3j6YaFIP8MOvfYsRX+0Dkl+4Gsi+vEL6qddZBq8uwjoqbqjJBb4tXLOHYj7ZBn+5yCPi4E/WoSk8nj26Y179/i4g4+u+hx1CMByuBJUSVCbzUgmqwAxSgiowX0xiLzRBJY+z9K0D9POFDceIuhp5ZROdgWT0o3iBbA6s8LdeA3+iJ+6Arsp3lsuvYMCIklrofp7dC2tgwQlY3TRe2QXZBCysiwTiOls3Wjm1Lh6/cqRxWh6e8+ZsUGl692H4V83+I6xZ5RVAiqHrNgyCiY/WREcBYSbw/UTdYvGiCp+lv3LmrprvhzpVD0gnRwc9miAXGWdBqPwk7Idjd0AXOG44rKiL7sK4piUgXp47XFp3Gs8z/alvqYq9+8Evn3+ZWc0dI6ik1UdeKaSfb3yMnYfbxfNRMphR1qnanfD3k7Oy5oiWx58nsJt1tR63IE2m4SJ0Hl8b37YyfDBuYfnz/BH0RL16IGx8vPOGqKQjSlAJJ/RUXnwlqJSgapsZSlAZHXv070uHh45W4BT5rD/upLaKD0Nie9xY8Uw7wFahPunQ3ayYM5SKXNIfOg3T8oYMFaexsr68D9bArUd5JWN/nha27onfi6F4xIN29udK7g5k8uRlfamNxFisPOKh/upfD1P84nXFRAXZtF6IRGHr/4BQo6JR7rLh8E+7aiT4eWkO/F7S+F4vqVeQVSOfANhdgjOUnxWcoixFx8HHb/YC+TU1Ybx8Zz+BrOxREEx5OejHU7MwnjnpQAbS3tnS+gbMM0FUe/YBUXcWRPXEa9A1vrIROw0fEjUgUCMjGEnFxgAZDR6As3Xd48Bv8bMzFvOGvVsBlG/kbxCUlGL8Kqt4x8BIy3sFvmVrKbfEO5X4HjyvZw2ihB+MSfd25cwf5x1RKUF15nD4/1aCSgmqtlmhBNV5RlRyR/PSN7GCvLjhOL2t4k9k7pELyS+6qul8BvDhqXn+b30IMQ1NsL6s5ZtCNx0CEmAjXCug4pXNzyoYQiPtZeW9n4P9p8amA0HdPyqTSsmRqpp6IIO5z0DX8n87sfJ5WiwiUm8foHNJTsbKe+d1aZQydSLa6xlEd+AtbvKjug5IdeO2Msr56ielRA8cgn+XzY7ny8zACrvkbozfqOzwkLFJd7RGF3Rk05aAb7sK0Q9PM6hZ+dbvNVGWSLsnSLv//voB+vny24KoMB/N3ge5kXVILvi57B5Yw7NSgEjlfZN2jJQ1VN4NQ0MT5tGX+6oo64ZtsJb+bQfCNTWM2EO+DQUt253o59XjYK1fyf3161frC2eCJY1FIhsWxilBZeCrElTEECWolKBqmwjnfesnr+cBvtt85lPQVZWVYwXxtAAxSL7gFDqN7P7RlGXp3bASDc0Kza/KWL8I0reKsZJsZM/h6kasPeKZ7vVkN1YQZtjOZ9ii+Czhv4xKpZpGp0LnINUeqwQCmP6HXRR1/LhYr0AlX3CK53BwO3fdDB3BvT8azEVkjQ1eQzgp+UXfUbFlbxUR3X8EOqJH78il8DX8vOHUbaWMqxlIQBDVTkZU1pXWHYuonly/nx5jLe8wPFpoiGp4HpDKn+aMpHoiZS0VP691fz1C9a566yjRmmp5T0PTiQqSHp4H3eTL98L6142/y0mVt/5Tgko4EYQqQaUEVeCpoQRVG18uOkElntV/eAP+I2v/Ah2Gz2plskNla4e4X835cSbNr/k3Y4Vu/bRv4PkWYuzXfCPj84VAWOUNmLBuvl1BPMq9m/wQ65fs9iisiCm4glr7/bgsSkqKhQ5J8u0pATKZuXQvRVVWsm7Kos5AdBpZmUCiz8yF8jo3Q4/cpL1I05ITsPqWVABRTeSv0US6HWN9TV5ElU9JuwrRfldBVJewf9Ky2cPp+fom80QyMiLMcAvfhnD/i7uphnc/g87KzV+BMvfX4obZap/C339cPgdIfkxukq5nnQZRKUGlGxdNCSo9PyIdUoLq7Dh60QoqYdvOYiCEXyyHzqXipFgVZA8sOY1UEBNo/yzseVfOHUYZB1q898ZYa7DwgRrohl4rArLazn5Cbj4iIMjKw7chBKvHGC9nA21s1pvM905NyYEnutxrJeW28L1aC1fDSlRTI7oMppIxCJX7nsbx99meXwAdgVPMikHKXejRSlBFZgS3FVZSRQue2UO0qopPHFhE9HKjq9wyMmcydg6/vDFb18FOg6ikV0pQwTyvBJXMiI6hSlBFhq8XraCSCfS7/4auav2H+CqHz/rXvq5KjpyIyuqhO7NpRKZd1T8yI2OoRb5y89Fx+Fm9cQB79co6IBpHNHRNLeyPIrcj+PbwggRRsdy+4LQDSf7ruH6UcEnvwDqG1zYfovQn1oE21At/eGUz9NcXRLt2vr9rMp+VfGwadFQOhah8rAr4q3Mr0ztaRyUsqaiBTvT2RTso6vBRhC378fE8s/G9a9P+KYPqeWQq65a5oU6HqJSggrJcCSp5FTqGyjybtkQp08+GwxetoBKmbdkDJLVwJfa+tbVAAFbPAAqyys0GElk1Dx66/SxbP4BMxE/K5j3UJD0MTIv5bvg3+b6tr8uhy6pvAEKy8w2hbr4b3c1+YnJXuzMGurW8RKzYD4zqQw314LvXja0+9+5+ilqxHv4sriZBVGjPmN8X1iOqW66GleWJO8CnCBlJfc11sl9KUEVmQMr5loxZSyHwC4tgPfXtgEza8SIq+EH+7GbM9wdv6+SISh5LCSolqGQudARVgioyXL3oBZV4DoufxgdboPtp4TvNTdnsvSESyOFXUzOpyD9fn2Na9MwMdXXHKOjRgIx6dNdL+jPznvm7ka19h/ieq7cPof+7qrCHr+OvoWh81kl0U26+WXTqIPgxTc5OPLNav99vb4WH8ONriymtrk6sLmY6KlQlVr/LRuAs2Jr58GSOiYKg9Guwi0QoQRWZgRRBNXclTpTkF8hZUzMrPbfvRVSYb9NvAqJ6+McDdR3sdDoq6Z0SVEpQyVzoCKoEVWS4etELKmGj3G29YFUBRVWcZP8gi34aghgGD4D1bfU8eOpmGO5TkvaMtKGhjKIOFv+ZaO/eE4imp15JVL5GQ4F2/gnCOnkaK81Hx+Av9hV/t6/OA7eEeBuQ0C+HplBtuYavzBib+Oxb9O9Xzx6gpNra0PgjXwdJT4dn+r/9DIjzyhHpxqa6VFgJqsgMZ9l3OFN655JvqMJDR7BjCN3qBx3V9BsZUU3R71w6LaISNipBhS2Z8MNIlaAycsRaWAkqa3wyy6UEFXOooQkIY95q7IH/vh3+StbvMMfel69W1+ZPyaKaZ98wwGwMdOnHSt+n8L79K4j2ybiOaGbfnxLtFgvPcQpY+ce369S1wDp3mu9HctpgtUvgGzzNqio8An7M4rvST5RjhbN+Vzp0eBrrCsYMi6cm7/tRNtGxA1lH1sXMgEpQmc0sa+lHK6C7nbYYVr+yMkZU3jvWTeoRHRVb1WdO6kcF7rtV/352ekSlBFX7A60EVfv8CZaqBFUwzoQWrwSVgV+ffANdzH2rCymlnj2wPVZ1VXxKe8wl2Eot+wX8hVJ6QndlaM4v2NSEM0078u+ltKrq/USTEqHzyuw7hcKpyVcRtfNKQYEO/CfQezp7Bh85Bh2YVX8zb9fElZ/dr9LS4Hj6k2ugM8jL7EFZs5LBr4F9ZEt6YVoHlaDyjvxZ/Vj3SQmVX/wqaCPvgHzf5zSpns/Gdo8Dsn98BpDUTeMydQU7PaKS3ipBJZzQUyWo9PywGlKCyiqn2s+nBJWBP3I3+AMvwVP94y/gl2RdcmPlj3JAB/TYzEHUwuQJQAytdwgaWgwcLC39mBJ27v0d0ZgY1OdyAYGkpVxD8UlJ44kmJlxBNDoaHucUiOC/U+yPtWD1t1Trlh3ix8K6qpDbslMJuYPeZoMVMToa8dl9YB0cMziB8iX3xHf2enWH1cbGfMxJQ/yobCCvuG6CXK3xOeRuh1igqwuq4Xng/7LZ2Dn0SQp8VjREtnmzy+UB81ftpbjjpYLkQb0ZzX7wTmdILt6PZ+cNoRJpvYHgpfgFg6iUoJIh01MlqPT8sBpSgsoqpwLnU4IqMF+8sZt3nqDfC1ZAkp/G0aLWLzSy/5A3p/GHrORABmNHQGIvvRsrTqrFL/A2NkJXtWvPI9RAVQ3uzZLa5aSd5gHySE2+nPLFx+cS7R4HnVZCr9EUjpJPIFMo/H8bt8GD/kH2pxKHWcuIM1jT7OHvO+sI/mlspRFjoKcF1h95/rQUIMzh/B3AlAQgqsR40BF8X9hVI5KoZd+XfIN1JLLxXV1QjRoKRPWnObhfTJBvuFxsYev0ph3QFT/37lGqavd+Hnf+zp/vVhBrLdkcmA/jR+F9fGnhKBSUF4qruWAQlTy2ElTCCT1VgkrPD7OQElRmHNKnK0Gl54dpqJY9ux94EYjq4y/gR2T84m7QitjKEOXE2r+Y72i+YWxG0CKBEo4df4+id+9dTFS+EiJfonU4sCS08BdkPR6Eu8VAt9Mzvj+Vi+K70RMSJlK4b8YPiVr1eKfMrf/KvsPKNns5dFUFRUCYnmaBnJLzbKksdUwFYGn8w1s9WwOZ395oD3QYvRNQfsJI8OPyIaCTxqdTVqeDy3sLRvZHVxVUGiPgtFQg+okj4QcXFwt+hvpxPDFeV9XCP2rzduwoqmtkPFgnFeqXknm+OPg+tOk3pFGFD04ZLBXr6AWHqJSg0o2fN6AElZcVln4oQWWJTeIHrClBZY1ffrk+5r3yIy/to7RKOQMon6HxKyERWMltvGKPHQ5P7OXsV5XYAyuR5A5GG5tOUdLOXb8mWlUNhOdyiZYGJcU9SfyqBGl164YVTm47GDL4PirQN+MmojZR/qAa0//yHdmX2a9l0drDVMbtZscoUx2eaRNhZjAiMOY/f2lYdF3d43AC4ftjelE7d1wND+VLBwJphdl40GJdVlDxE8tV1h6NrbEW71PzZ5h+Pmv8ZXCf/6Ih3b+CgDHy9aNU1mU+u3Ao5RuSifE3FrrgEJU8gBJUwglQJaj0/DALKUFlxiFJNwgiJaiEMdbo6UboYO5/sYAKbNp6kqj/neRB6mPdSRy7Nz19Tx5lvGoU9spBSvlFHyvdRHEFhdBVuZqhKzL7+ExUFHQ68d3hzzV29NNUjzOqp18boUSIu8Kj6+DB/86n31FxD+uGztoKGEpnrOQVyMk6C7sdiDazDxDnf8wAfyKNrLq6oPL6BYqSycpYBMwjiJgFliD0gHktRPJ4s2pKm31LJhWay9/fDLaTuGARlRJUgSeFElSB+WKMVYLKyJFgYSWognEmpPhtBUBSC/m+qsoqIC1zvyo0Ix7YV4zG3njZbOyV47tZ01U1NdVSRTvy5xGtrj1ItLnZAJnRnCY6KrHq5Q1cQClZ/W7hHDIxOBgmKT0JHdpvXgDi/HIXPNU9HuaPBp2QT6UXuL9hNh9+MbmZ1QE/oNwsIKtl9wyjOnPS4sKv+4ySXV9QnfGwneinzYktTL8+mOfPzocf44A06IqDdfWCRVTyQEpQCSf0VAkqPT+MISWojBw5N+FwBdX/AwAA//+wbSHhAAAQXklEQVTtXXt4FNUVn83mTcIrTwJIAiQQ3sqrFa2tIIgVtVT9tLYVKAVFQKp8tbbWivVTi9jKW3z1Iba0voV+Yvm0WD9sRSs2USA8khCSkAdvDCGE7DSe3znDt2GXnc0mu5vl5I89c+/ce+fObya/+c25595xmM1/Rgf+a2xyUe9/8uwXZN/991GypqvR1lk5omKoXFw8ij82sx9tTB6dZau+FKqo3EibO4oWk3WZZ8g2NbnD63Q6KD8hvjvZ4cOWkO2c1J9sW/+UHayjJpe8WkL2w/8dIVt30knWbGKcorifLuBJO0P5ExVFR3dExZEdPzaZ7OJpA8kmxkeTbe3P6TM4z9uWFFATX+w6RdbVeNJmk8AvPhH2odv7UL0bLu1ts/75iz36yh4q8OL6A2RNA/eT4XK/n87fSjjsxf3ucOI6xsUB94dn4P/surE9bHXSoUSlREV3ihIVwaBEZYs3/CikROUG1qd7DlN6zrLtZI8dw5PHtrJixr9idGeq/8QMPLmTE0BkbgfzkKg/VUu5BYULyR49UULWxYpKlJQI2Jw+N9P+3H53kXU4oCAo0Q4/J0/hifzmR5XU+vqPDpLdUfwl2VMNsTgqCyrTOI20wRnWkzy4T3RHFJRTHB7IxiMzoTyvHeOf4uWTsYwqKguK9tmIAkEZBu7ruDgoz7umQnHOurqPX8ft8IpKzlaJSpDwbJWo3HFRonLHo81TSlSeIa1vaKIdD760k+yG99lXJcrAUgSe6xvy5I6Fgnh89gAqOHmUvXdo8fSVV26gejuK4HsyRJEYUCIxMfC1DB30EJVLTRlDNtg/R76ET2b91ho69Ce7T5Atq4aPprgMvq3GJjjvxJdnmMDHdAFvSRviQ+ETMS1fFyuyAE9QfFUTvt6FWhLFGx/bOl+VElWAF+Sc6qyg+P9I3hDSUlDwtquggGdOzKYMp0Vk5zTkMSNiFJUSlcfr6zVTiQoEqs50r7eInzuUqPwCTF4B71lTRPWqqqAc+FW5WeB487HIOzXepb99BUblHvkBfFUJcfae3PX1VXTcwu33kz1+Yq9b/9PTLqP04IG/Jut04nhuhUKQMFkpHTgMJfVPHh3cUw1fVd1pKayT9RglPHIcuFYfxv6KA7CGIefj7nMzDR5dtJSWnyfJo7Odk3GdltyB6/KNIal+NoTiqqj8hU3+P7iejMqKb9WUUVrcB6MGQ/ne9i28kUwamckVW7RjsxsRo6jkfJWoBAn/rBIViFdH/bzdNy0IRonKG1D28hvPYHTr4XW7qcLLmzAaZ/mKfD3RrSc3jrdy3iDaGD2AX7btdcMoK3+NSu7ctZRsTAyGrfLzfkrpzIwJNlsKr2Imx4cdPt5AHdtXU0/2s1L4thrOQLHWHEX+po8xGnuwlpXtWWnr34nxP4bJo6i/mJFL9X94ZS//2uHSka+oQCwOp7wJtCAan6jJm4fUE4UMa5pQTtFOlBuSl0gtXnUJFO7kkemUzkrp5PNIdgpEnKJSorJz2VtfRolKsMMrbvgGfCpRyZUKa/vpHoz6LXx2J/WzshIKwDB4tMpr7/kC8xN80jj4qh69HcoqkeNBvFbnHfX1h2hrW8EcslEcYT3y4jWUjolO4JKRaoDz+wVQVC9tLqcT3bLtGNkm8RWKtQmDjD5On5JBNRZO7U82iq+XzWaMSFdUMbEgUgcLItMQZWQTIRZUiZ3QzrB+UExdkpDum5lEDWV0w308uj9Gsy9KR77No9guFnGKSs5ciUqQCJVVogoE+UCn0ChRBYJ+COo+8OJ2Ouqr70LhmE3wYRkc1+StS/Lk7tIF7+Sr786nohf3g8LyVq9lfln5W5TV1ASfTXafmyjtsHw1LWtEZnrnfswxvO+FXXSCu4rh47A7c0BQkTljIwfB5/fkrMG0K6Orfwo1UhWVRPJn98ZMg2mTLiJ8UpKRdvm47wVnceUmxUNBDcwC3p14jmVCPNJSvr1txCoqAU6JSpAIrVWi8g//1ioqJSr/cA6b0p8V40l+96od1KeaWryS2H2SO2Mw12/yZVBSv5kGX5XdyNomVnAyxy862t7cwbABsI078vtN+6jFxWtLyZoSuW/TVyX/iJkZGM1aNW8ItZPfuzNZuz+Rq6hwfw3Px4yC1XOHEiTdkpC2i0+4lYt4RaVEFV63nBKVvevRekWlRGUP4TArJUpm0V8w+vfXd9hXJXPT5GXcW785rirLeoLDVzWwd1dvNTT/PAi8//lB2rtgOXyH9RJe5RLf4XkqN+8SRZXYCb7DNfdCMYzqh0jo89c+uzfSFdXgPPiQls6G4uyZilG7swh0rK2IV1RKVOF1QypR2bsegSoqJSp7OIddqYJSxO/MW4EneXUtx1X5UlQcfyK+qu+OT6Nze/CWXLLRTo7Y5biTkurjlB/FASx9MpLDDotQdmjnfsS3LXga16G0nOPabK/ICt9UfAIrqnsw6jcmr5tfp6WKyi+4Ql444hWVIKxEJUiE1ipR2cNfFZU7ThcMUbl4/aQVG0oIgVWv7mckWBHJ+kru+FgpWQ+pO4dRrZyL0b8R/fAkP3wCCu2+F6AURuUif/Y12VYbumEYZ4kKo7Cl5eyb8lNRqY/K890k8X/66ucZn7DPVaIKj0ukRGXvOqiicsfpglFUcto79sNXNXcllE9FBSKk7c8BhI9k2pQe1OR9N/Yju7kQo1lzf/sFpadf35vsvTdgPyWC+gOnWUExzrdHCuJo0rqENp5mM4/63b0MODWwq9DwoWgFOhn165WFYfiVHEeVl+WfL1B9VIJox7BKVEpUQb1Tlajswa2Kyh2nC46oZI7f797YQ0g883oFWR60a162ysca3xxX1SPDSfV++b2+ZF/ZUkX2s934qssTs+DDGjeInVq0N3g//yqspoMtWltMdkRuEtm51+WQzWmnWe7UuMcfILxsPXyEq1+Bj/Ds2urWFfBYWzJlrt/YoVCGS2YC59QuOtfvK4zURyV3Soe3+IdQogJxBe9yKlH5g7UqKne0LkBFBQC2lyGeZ/5qRKyXV7KvymeENNb1cUbD9srCk73yAEKsr748nQ6weBoi2B2yIJA77u2WKizF+k8/ewErnO7dh7XKHbxm+fB8zIlbcH029eFr+aL4cD7t1bHXP8T3BJe+XkqHqK7BaJ/pE2/ukfXVEijZ6ewjXDgVPkCnrkdFQKmi4vslUowSVTZdSiWqAsIhUj7prkQVKQzV4jyefA3K4/n1B2iP64z4qMS2qCBJfsI7DCiq5GREWD/+Y6w4eeXwTCkZFFtaje/y3f+HIjretu1Y/8oaTbP6i1HL9DTEj00cjbXgJwxD3NfIXMxhDHSVh7p6DOet+wC4/mkTbHWNTO5jn5QPmC3woqCkovjT8w/NyKNdN1+eZRXxZ0NH/fxBK/RlL9hXP4FeiUqJ6qt7QRWV/EeEp73giWpXBZTI7KWIq6qqhk/HdEl81fkvnMOJlRPHDMXXNp6ZP4wqxMVAAZy/duB7a46epEYeXAtluPljxE3J6KalqORQlrLC7Hr5Ok96GnxUQ/rCh5XaFfubWPGkJEOJfXMw9kezj67iIJTTJ8V1dITT/BWaylr0a2shfIENp9CQ3y47njhguIDngP4Y3XtiJnyAeT39i58SGFRRCRIdwypRKVHRnapEhVfSjv5dP/VRdQzibXUvV23YS3VXc1xVo/iqvMVVsc8kMRGP/EXTMPo0ZWzrfCb+dvxYHZTMYy9DSb3xbi2akNEvm5HeBpd3GFCGkjZMXtWAv4TrMKAwM9KgrEQZ1Z2EUjrxJXAwTR49tMKieHRPvv5jcyVPCw+rf2j/7IwA+AKbI4esov5sqKLyB63Ql73gFZVcAiUqJaqv7gVVVPIfEV5WiYqvx94q8VVhDprPuKooKIvcHIz6Pb8AK02m+/k1FH9vhxP1UDZPvVlMVf/8dhVZU75qYzcuyeuBWaFYcUtSUJQL5thJruUL4zgtK99STpa0snbZ25B+wDeV3QtEumwOItHzesJXZq+tc0upojoXk3DOUaLiq6NEJbepEIQQU4t8Q4kKiIBAw+1Lyeqjkvs1Qq3LxJP/uY1QKk+tK6MzdYkPRHxV7JtycjzP/JuwSsKPJuVQeWvFz3bCae8BzCV84I9Yz2nbDo6Xku+12fVNtVP/Am5WlJwLRJmUDN/UjGt7UtNzroEvMNDjqKIKFMHg1ldFxXgrUQX3xvN6NCUqgkbn+rnfIUpU7ngYew/AV3XncviqyioQV9XsZUVJHoVK6Q6fyar5iOcZnoPI7hbNtXmyoRGjbLIA3bK39tExtmxDv60DBuyrsloKzgaEU7M3G69UsXFQuDeOR4T//Cl9qR9dk9jpH2CvVFEFCGCQqytRtQBciaoFIMFKKlG5Ia2Kyg0OQ4nKHQ8rtfrv8FUt53WTXOwzMZsQX3TrNT2o7P03IZ4nLgajgFYDQdqoOASf1a/WIg5syzakTf5Cs8G+NJ/rbAWpv+ccxoqUh5M+2om4q2uvSKWiC64HvhldMbp6Tv1WZqiiaiVwIaqmROUFeCUqL8C0dbYSlUdEVVG5w6JE5Y6HlSrhuKpZPAewvAqjUAlxmGqxePYAKjvh4gyrTig3Kg9hbt1zm/ZTN975CGu4HzoscUwcaW6NDsLXFfQ+i7PcgC9Kjp+AKXzG9yfCJ3X7BIympnbmHVKwjawqqjYCMkjNKFF5AVqJygswgWYrUdlCUBWVO0xKVO54nJN6emMJ5a14uZLsyEGJZNfMHUI2Pq5lACRlh/AHCmrjf2uoD2Lf23qE0qcb4LV2OKEQDRPKyvQ5F0+Uma9T43Y5/swhcw95ciCHqzU3gvZG5APPG3ldqamXQlFF8RxDX0dr7X5VVK1FLjT1lKh84K5EJQApUQkSsHh11ch0d1TaK6VE5QPZikNYZ+mOZYVUcuKodLLzOK7HR/WQ767juYFvbYXCkvW33vsUCqu6FnMH5UvQhqx+IL4sQ3xZdomK4wxYEZku+PRiY5B/ST7Wjxo3GHFn40dgdK9vZnA/NiGK6pbFuK5FJfDh+TspOToa9RZNu4iu9dTLYAO98I/8DativPT2ITRlYjTUOCtJPR+Cv5I0IAfKdsUczEHtlQrl6rlS+OcqUfm4RkpUSlSebxEoKiUqz+i0da4SlQ9EGxrxxPygEOs9DeiFJ3/voH8Xz0dH7e7miPX/7MZKoGWsqGqOYs5gaRXisM7w6gdFZcivrIHyksPI13XkAS8rfuZdhMjxQX2gnHJ6AK+unZB/SV+ks1KwIqq0F2zbyEuXTn/qczp0QRGUc5SDlYuPDsm6W7GxUC4/v7UX1fjOuD4+atrb/fA6KKp1/4DyNW0qKpmUnJeN4yy/czBt9E4LLd72ztp7KSUq79jQHiUqJSpPt4gSlSdU2i9Piar9sO2QLZ88hZVDZbGIiiNIVx2BohIFJYpKJnPHRsMHlZ2GtdZ7doc1eN2ucAND+l1Qcpy6Vnsc5xkb7R7f5a3fLlac0Tx6mpsFH1Bmt7aJ+yqqgLItKofSszsI2sT9yuwCBTuiL74qFB/LvkNvJxTm+UpUYX6Bgt09JSolqmDfc3aOp0RlByUtowgoAiFFQIkqpPDrwRUBRcAOAv8HrolWcFJig4wAAAAASUVORK5CYII=";
    private readonly IStatementPdfTemplateReadOnlyRepository _repository;
    private readonly ILogger<StatementPdfGenerator> _logger;

    public StatementPdfGenerator(
        IStatementPdfTemplateReadOnlyRepository repository,
        ILogger<StatementPdfGenerator> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<byte[]> GeneratePdfAsync(PeriodicEnergyStatementEntity statement, StatementPdfTemplateEntity? template = null, CancellationToken cancellationToken = default)
    {
        if (template == null)
        {
            if (statement.StatementPdfTemplateId.HasValue && statement.StatementPdfTemplateId.Value > 0)
            {
                template = await _repository.GetByIdAsync(statement.StatementPdfTemplateId.Value, cancellationToken);
            }

            if (template == null)
            {
                template = await _repository.GetDefaultAsync(cancellationToken);
            }
        }

        template ??= GetFallbackDefaultTemplate();

        var stream = new MemoryStream();
        BuildQuestPdfDocument(statement, template).GeneratePdf(stream);
        return stream.ToArray();
    }

    public Task<byte[]> GeneratePreviewPdfAsync(PreviewStatementPdfTemplateRequest templateRequest, CancellationToken cancellationToken = default)
    {
        var template = new StatementPdfTemplateEntity
        {
            HeaderTitle = templateRequest.HeaderTitle,
            SubHeaderTitle = templateRequest.SubHeaderTitle,
            CompanyName = templateRequest.CompanyName,
            CompanyAddress = templateRequest.CompanyAddress,
            ContactPhone = templateRequest.ContactPhone,
            LogoBase64 = templateRequest.LogoBase64,
            PaperSize = templateRequest.PaperSize,
            DisplayColumnsJson = templateRequest.DisplayColumnsJson,
            FooterNotes = templateRequest.FooterNotes,
            RequireDualSignatures = templateRequest.RequireDualSignatures
        };

        var mockStatement = CreateMockStatement();
        var stream = new MemoryStream();
        BuildQuestPdfDocument(mockStatement, template).GeneratePdf(stream);
        return Task.FromResult(stream.ToArray());
    }

    private static IDocument BuildQuestPdfDocument(PeriodicEnergyStatementEntity statement, StatementPdfTemplateEntity template)
    {
        var isLandscape = string.Equals(template.PaperSize, "A5_LANDSCAPE", StringComparison.OrdinalIgnoreCase);
        var pageSize = isLandscape ? new PageSize(595f, 420f) : PageSizes.A4;

        byte[] logoData;
        try
        {
            var logoStr = !string.IsNullOrWhiteSpace(template.LogoBase64) ? template.LogoBase64 : DefaultLogoBase64;
            if (logoStr.Contains(","))
            {
                logoStr = logoStr.Substring(logoStr.IndexOf(",") + 1);
            }
            logoData = Convert.FromBase64String(logoStr.Trim());
        }
        catch
        {
            logoData = Convert.FromBase64String(DefaultLogoBase64);
        }

        // Parse column visibility
        var columns = ParseColumns(template.DisplayColumnsJson);

        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(pageSize);
                page.DefaultTextStyle(x => x.FontFamily("Times New Roman").FontSize(isLandscape ? 8 : 9));

                page.Content().Column(col =>
                {
                    // 1. Header
                    col.Item().PaddingTop(isLandscape ? 12 : 20).PaddingBottom(5).Row(headerRow =>
                    {
                        headerRow.ConstantColumn(80).Width(80).AlignRight().Image(logoData);
                        headerRow.RelativeColumn().PaddingHorizontal(10).Column(center =>
                        {
                            center.Item().Text(template.CompanyName).FontSize(isLandscape ? 13 : 15).AlignCenter().Bold();
                            center.Item().Text(template.CompanyAddress).FontSize(isLandscape ? 7.5f : 8.5f).AlignCenter();
                            center.Item().Text(template.ContactPhone).FontSize(isLandscape ? 7.5f : 8.5f).AlignCenter();
                        });
                        headerRow.ConstantColumn(80);
                    });

                    // 2. Title Section
                    col.Item().PaddingHorizontal(10).PaddingVertical(isLandscape ? 4 : 8).Column(titleCol =>
                    {
                        titleCol.Item().Text(template.HeaderTitle).FontSize(isLandscape ? 11 : 13).Bold().AlignCenter();
                        titleCol.Item().Text(template.SubHeaderTitle).FontSize(isLandscape ? 11 : 13).Bold().AlignCenter();
                        titleCol.Item().Text("(M&E Copy – Liên cho Phòng Cơ Điện)").FontSize(isLandscape ? 9 : 10).AlignCenter();
                    });

                    // 3. Customer & Measurement Point Info
                    col.Item().PaddingHorizontal(10).PaddingBottom(isLandscape ? 6 : 10).Row(infoRow =>
                    {
                        var customerName = statement.MeasurementPoint?.Customer?.FullName ?? "---";
                        var pointCode = statement.MeasurementPoint?.Code ?? "---";
                        var address = statement.MeasurementPoint?.Address ?? statement.MeasurementPoint?.Customer?.Address ?? "---";
                        var serialNumber = statement.SerialNumber;

                        infoRow.RelativeColumn().Column(left =>
                        {
                            left.Item().Text($"Customer (Khách hàng): {customerName}").Bold();
                            left.Item().Text($"Address (Địa chỉ): {address}");
                            left.Item().Text($"Point Code (Mã điểm đo): {pointCode}     Serial No: {serialNumber}").Bold();
                        });

                        var fromDateStr = statement.PreviousDay.HasValue ? $"{statement.PreviousDay:00}/{statement.PreviousMonth:00}/{statement.PreviousYear}" : "01/10/2026";
                        var toDateStr = $"{statement.Day:00}/{statement.Month:00}/{statement.Year}";

                        infoRow.ConstantColumn(200).AlignRight().Column(right =>
                        {
                            right.Item().Text($"Period (Kỳ): {fromDateStr} - {toDateStr}");
                            right.Item().Text($"Meter Multiplier (Hệ số nhân): {statement.Multiplier}");
                        });
                    });

                    // 4. Data Table
                    col.Item().PaddingHorizontal(10).Table(table =>
                    {
                        table.ColumnsDefinition(def =>
                        {
                            def.RelativeColumn(1.8f); // Tariff
                            def.RelativeColumn(1.2f); // Previous
                            def.RelativeColumn(1.2f); // Current
                            def.RelativeColumn(1.2f); // Difference
                            def.RelativeColumn(0.8f); // Multiplier
                            def.RelativeColumn(1.4f); // Consumption
                            if (columns.Contains("PMAX")) def.RelativeColumn(1.2f);
                            if (columns.Contains("COSPHI")) def.RelativeColumn(1.0f);
                        });

                        // Header Row
                        table.Header(header =>
                        {
                            header.Cell().Element(CellStyle).AlignCenter().Text("Biểu giá điện\n(Tariff)").Bold();
                            header.Cell().Element(CellStyle).AlignCenter().Text("Chỉ số cũ\n(Previous)").Bold();
                            header.Cell().Element(CellStyle).AlignCenter().Text("Chỉ số mới\n(Current)").Bold();
                            header.Cell().Element(CellStyle).AlignCenter().Text("Độ lệch\n(Difference)").Bold();
                            header.Cell().Element(CellStyle).AlignCenter().Text("HSN\n(Factor)").Bold();
                            header.Cell().Element(CellStyle).AlignCenter().Text("Sản lượng (kWh)\n(Consumption)").Bold();
                            if (columns.Contains("PMAX")) header.Cell().Element(CellStyle).AlignCenter().Text("Pmax\n(kW)").Bold();
                            if (columns.Contains("COSPHI")) header.Cell().Element(CellStyle).AlignCenter().Text("Cos φ").Bold();
                        });

                        var mult = statement.Multiplier > 0 ? statement.Multiplier : (statement.MeasurementPoint?.MultiplierFactor ?? 1);

                        void AddRow(string label, double prev, double curr, double diff, double cons, double? pmax = null, double? cosphi = null)
                        {
                            table.Cell().Element(CellStyle).Text(label);
                            table.Cell().Element(CellStyle).AlignRight().Text($"{prev:N2}");
                            table.Cell().Element(CellStyle).AlignRight().Text($"{curr:N2}");
                            table.Cell().Element(CellStyle).AlignRight().Text($"{diff:N2}");
                            table.Cell().Element(CellStyle).AlignCenter().Text($"{mult}");
                            table.Cell().Element(CellStyle).AlignRight().Text($"{cons:N2}").Bold();
                            if (columns.Contains("PMAX")) table.Cell().Element(CellStyle).AlignRight().Text(pmax.HasValue && pmax.Value > 0 ? $"{pmax.Value:N2}" : "-");
                            if (columns.Contains("COSPHI")) table.Cell().Element(CellStyle).AlignCenter().Text(cosphi.HasValue && cosphi.Value > 0 ? $"{cosphi.Value:N2}" : "-");
                        }

                        if (columns.Contains("BT")) AddRow("1. Bình thường (Normal - BT)", statement.PreviousBt, statement.Bt, statement.Bt - statement.PreviousBt, statement.ConsumtionBt);
                        if (columns.Contains("CD")) AddRow("2. Cao điểm (Peak - CD)", statement.PreviousCd, statement.Cd, statement.Cd - statement.PreviousCd, statement.ConsumtionCd);
                        if (columns.Contains("TD")) AddRow("3. Thấp điểm (Off-Peak - TD)", statement.PreviousTd, statement.Td, statement.Td - statement.PreviousTd, statement.ConsumtionTd);
                        if (columns.Contains("SG")) AddRow("Tổng điện năng hữu công (Total kWh)", statement.PreviousSg, statement.Sg, statement.Sg - statement.PreviousSg, statement.ConsumtionSg, statement.Pmax);
                        if (columns.Contains("VC")) AddRow("Vô công (Reactive - kVARh)", 0, 0, 0, statement.ConsumtionVc);
                    });

                    // 5. Pmax Detail & Notes
                    col.Item().PaddingHorizontal(10).PaddingTop(isLandscape ? 4 : 8).Column(notesCol =>
                    {
                        if (statement.Pmax > 0)
                        {
                            var pmaxTimeStr = statement.PmaxTime.HasValue ? statement.PmaxTime.Value.ToString("dd/MM/yyyy HH:mm:ss") : "---";
                            notesCol.Item().Text($"* Công suất cực đại Pmax: {statement.Pmax:N2} kW (Ghi nhận lúc: {pmaxTimeStr})").Bold();
                        }

                        if (!string.IsNullOrWhiteSpace(template.FooterNotes))
                        {
                            notesCol.Item().PaddingTop(2).Text($"Ghi chú: {template.FooterNotes}").Italic().FontSize(isLandscape ? 7.5f : 8.5f);
                        }
                    });

                    // 6. Signatures
                    col.Item().PaddingHorizontal(10).PaddingTop(isLandscape ? 8 : 16).Row(sigRow =>
                    {
                        if (template.RequireDualSignatures)
                        {
                            sigRow.RelativeColumn(0.48f).Element(PartyBoxStyle).Column(partyA =>
                            {
                                partyA.Item().Text("Party A (Supplier)").Bold();
                                partyA.Item().Text("Bên A (Đơn vị bán điện - VSIP Hải Phòng)");
                                partyA.Item().PaddingVertical(isLandscape ? 8 : 15);
                                partyA.Item().Text("Name and Signature \n(Ký và ghi rõ họ tên)").FontSize(7.5f);
                            });

                            sigRow.RelativeColumn(0.04f);

                            sigRow.RelativeColumn(0.48f).Element(PartyBoxStyle).Column(partyB =>
                            {
                                partyB.Item().Text("Party B (Tenant)").Bold();
                                partyB.Item().Text("Bên B (Khách hàng mua điện)");
                                partyB.Item().PaddingVertical(isLandscape ? 8 : 15);
                                partyB.Item().Text("Name and Signature \n(Ký và ghi rõ họ tên)").FontSize(7.5f);
                            });
                        }
                        else
                        {
                            sigRow.RelativeColumn(0.5f).Element(PartyBoxStyle).Column(partyA =>
                            {
                                partyA.Item().Text("Party A (Supplier)").Bold();
                                partyA.Item().Text("Bên A (Đơn vị bán điện - VSIP Hải Phòng)");
                                partyA.Item().PaddingVertical(isLandscape ? 8 : 15);
                                partyA.Item().Text("Name and Signature \n(Ký và ghi rõ họ tên)").FontSize(7.5f);
                            });
                        }
                    });
                });
            });
        });
    }

    private static HashSet<string> ParseColumns(string? json)
    {
        var set = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "BT", "CD", "TD", "SG", "VC", "PMAX", "COSPHI" };
        if (string.IsNullOrWhiteSpace(json)) return set;

        try
        {
            var parsed = JsonSerializer.Deserialize<List<string>>(json);
            if (parsed != null && parsed.Count > 0)
            {
                return new HashSet<string>(parsed, StringComparer.OrdinalIgnoreCase);
            }
        }
        catch
        {
            // Fallback to full set
        }
        return set;
    }

    private static IContainer CellStyle(IContainer container)
    {
        return container.Border(0.5f).BorderColor(Colors.Black).Padding(3);
    }

    private static IContainer PartyBoxStyle(IContainer container)
    {
        return container.Border(0.5f).BorderColor(Colors.Black).Padding(4);
    }

    private static StatementPdfTemplateEntity GetFallbackDefaultTemplate()
    {
        return new StatementPdfTemplateEntity
        {
            Code = "DEFAULT_A5_VSIP",
            Name = "Phiếu Báo Điện Năng Chuẩn VSIP (A5 Ngang)",
            HeaderTitle = "MONTHLY POWER METER READING",
            SubHeaderTitle = "(PHIẾU GHI SỐ ĐIỆN HÀNG THÁNG)",
            CompanyName = "VSIP HAI PHONG CO.,LTD",
            CompanyAddress = "VSIP Hai Phong Administration Office, VSIP Hai Phong Township, Industrial & Service Park, Hoa Binh Ward, Hai Phong City, Vietnam",
            ContactPhone = "Tel: 84-225-3959868   Fax: 84-255-3959886",
            PaperSize = "A5_LANDSCAPE",
            DisplayColumnsJson = "[\"BT\",\"CD\",\"TD\",\"SG\",\"VC\",\"PMAX\",\"COSPHI\"]",
            FooterNotes = "Quý khách vui lòng kiểm tra và phản hồi số liệu trong vòng 03 ngày làm việc kể từ ngày nhận phiếu báo.",
            RequireDualSignatures = true,
            IsDefault = true,
            IsActive = true
        };
    }

    private static PeriodicEnergyStatementEntity CreateMockStatement()
    {
        return new PeriodicEnergyStatementEntity
        {
            SerialNumber = "PB-202610-001",
            Multiplier = 120,
            MeasurementPoint = new MeasurementPointsEntity
            {
                Code = "101.402.001",
                Address = "Lô đất số IN1-4, KCN VSIP Hải Phòng, Xã An Lư, Huyện Thủy Nguyên, TP. Hải Phòng",
                MultiplierFactor = 120,
                Customer = new CustomerEntity
                {
                    FullName = "CÔNG TY TNHH KYOCERA VIỆT NAM",
                    Address = "Lô đất số IN1-4, KCN VSIP Hải Phòng"
                }
            },
            Day = 31,
            Month = 10,
            Year = 2026,
            PreviousDay = 1,
            PreviousMonth = 10,
            PreviousYear = 2026,
            PreviousBt = 124567.0,
            Bt = 125980.0,
            ConsumtionBt = 169560.0,
            PreviousCd = 34512.0,
            Cd = 34980.0,
            ConsumtionCd = 56160.0,
            PreviousTd = 56089.0,
            Td = 56890.0,
            ConsumtionTd = 96120.0,
            PreviousSg = 215168.0,
            Sg = 217850.0,
            ConsumtionSg = 321840.0,
            ConsumtionVc = 45200.0,
            Pmax = 1450.85,
            PmaxTime = new DateTime(2026, 10, 15, 14, 30, 0)
        };
    }
}
