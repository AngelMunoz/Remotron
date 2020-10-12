//@ts-check
import { createService } from 'fable.remoting.ts'

/**
 * @typedef {{ getTest: () => Promise<{data: string}>; getSample: (params: { a: string[]; b: number[] }) => Promise<any>}} IService
*/

/**
 * @type {IService & import('fable.remoting.ts').BaseService}
 */
const sample = createService({
    baseURL: 'http://localhost:5000',
    serviceName: 'IService'
});

(async () => {
    const r1 = await sample.getTest();
    const r2 = await sample.getSample({
        a: ['one', 'two', 'three', 'four'],
        b: [1, 2, 3, 4]
    });
    console.log("getTest: ", r1.data);
    console.log("GetSample: ", r2.data);
})();